#!/usr/bin/env node
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

/**
 * Automated Dependabot alert remediator for the typechat.net (.NET / NuGet)
 * repository.
 *
 * Strategy per alert: edit the single root ``Directory.Packages.props`` to
 * pin the package at the advisory's ``first_patched_version``. With
 * ``CentralPackageTransitivePinningEnabled`` (set in that file) a single
 * ``<PackageVersion>`` entry works for both direct *and* transitive
 * dependencies, so there is no second-strategy fallback like the npm
 * ``overrides`` mechanism in the parallel TypeChat script.
 *
 * After each attempt the script runs ``dotnet restore TypeChat.sln`` and
 * verifies that no NU1901/NU1902/NU1903/NU1904 warning for the alert's
 * GHSA URL remains. ``NuGetAudit`` with ``NuGetAuditMode=all`` is enabled
 * by the root ``Directory.Build.props`` so transitive vulnerabilities
 * surface here.
 *
 * On a successful fix the solution is rebuilt with ``/warnaserror`` and
 * the unit tests are run to catch breakages introduced by the upgrade.
 * On any failure — restore, build, test, or verification — the script
 * restores ``Directory.Packages.props`` from a backup and records the
 * failure in a persistent rollback-state file. Future runs skip
 * recently-rolled-back packages (default 7 day cooldown, keyed off the
 * props file's git hash) so the same broken upgrade isn't re-proposed
 * each night.
 *
 * Run modes:
 *   node tools/scripts/fix-dependabot-alerts.mjs                # analyze
 *   node tools/scripts/fix-dependabot-alerts.mjs --auto-fix     # apply
 *
 * Auth: reads alerts via ``gh api repos/<owner>/<repo>/dependabot/alerts``
 * which requires a token with ``security_events`` (org-owned repo) or
 * a GitHub App installation token. In CI, the workflow mints the latter.
 */

import { spawnSync } from "node:child_process";
import { readFileSync, writeFileSync, existsSync, copyFileSync, rmSync } from "node:fs";
import { join, dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { tmpdir } from "node:os";

const __dirname = dirname(fileURLToPath(import.meta.url));
const REPO_ROOT = resolve(__dirname, "..", "..");

// ── Configuration ────────────────────────────────────────────────────────

const PACKAGES_PROPS = "Directory.Packages.props";
const SOLUTION = "TypeChat.sln";
const TEST_PROJECT = join("tests", "TypeChat.UnitTests");

const RUN_TEMP = process.env.RUNNER_TEMP || tmpdir();
const ROLLBACK_STATE_PATH =
    process.env.DEP_ROLLBACK_STATE_PATH ||
    join(RUN_TEMP, "fix-dependabot-alerts-rollback-state.json");
const ROLLBACK_COOLDOWN_DAYS = Number(
    process.env.DEP_ROLLBACK_COOLDOWN_DAYS || 7,
);

// ── Args ─────────────────────────────────────────────────────────────────

const ARGS = parseArgs(process.argv.slice(2));
const DRY_RUN = !ARGS.has("auto-fix");
const SKIP_TESTS = ARGS.has("skip-tests");
const VERBOSE = ARGS.has("verbose");

function parseArgs(argv) {
    const flags = new Set();
    for (const a of argv) {
        if (a.startsWith("--")) flags.add(a.slice(2).split("=")[0]);
    }
    return flags;
}

// ── Logging ──────────────────────────────────────────────────────────────

const log = (...args) => console.log(...args);
const dbg = (...args) => VERBOSE && console.log("[debug]", ...args);
const warn = (...args) => console.warn("⚠️ ", ...args);
const err = (...args) => console.error("❌", ...args);

// Sensitive env vars that must NOT leak into dotnet child processes —
// build/test scripts can execute arbitrary code from newly-installed
// dependencies (MSBuild tasks, source generators, test runner adapters),
// so we strip authentication before invoking them.
const SENSITIVE_ENV_VARS = [
    "GH_TOKEN",
    "GITHUB_TOKEN",
    "NUGET_API_KEY",
    "DEPENDABOT_APP_PRIVATE_KEY",
];
function sanitizedEnv() {
    const e = { ...process.env };
    for (const k of SENSITIVE_ENV_VARS) delete e[k];
    return e;
}

// ── Shell helpers ────────────────────────────────────────────────────────

function run(cmd, args, opts = {}) {
    // ``dotnet`` and ``git`` and ``gh`` are real .exe on Windows; do not
    // use ``shell: true`` for them, otherwise cmd.exe would interpret
    // shell metacharacters like ``&`` in URLs.
    const needsShell = opts.shell ?? false;
    dbg("$", cmd, args.join(" "), "  (cwd:", opts.cwd || process.cwd(), ")");
    const r = spawnSync(cmd, args, {
        encoding: "utf8",
        maxBuffer: 64 * 1024 * 1024,
        ...opts,
        shell: needsShell,
    });
    return {
        ok: r.status === 0,
        code: r.status,
        stdout: r.stdout || "",
        stderr: r.stderr || "",
    };
}

// ── Semver (minimal — just compare a.b.c[-prerelease]) ───────────────────
//
// Sufficient for "is resolved version >= first_patched_version" checks.
// NuGet packages use SemVer 2.0; legacy 4-part versions (``4.0.5.0``)
// also exist but the security advisories we consume normalise to 3-part.

function parseSemver(v) {
    if (!v) return null;
    // Capture [major, minor, patch, prereleaseTag-or-empty]. A trailing
    // 4th numeric segment (NuGet legacy) is allowed but ignored.
    // Per semver, a version with a prerelease tag is LOWER than the
    // same version without (1.2.3-beta.1 < 1.2.3). This matters for
    // security verification: a vulnerable prerelease must not be
    // treated as satisfying the patched release.
    const m = String(v).match(
        /^v?(\d+)\.(\d+)\.(\d+)(?:\.\d+)?(?:-([0-9A-Za-z.-]+))?/,
    );
    if (!m) return null;
    return [Number(m[1]), Number(m[2]), Number(m[3]), m[4] || ""];
}

function semverGte(a, b) {
    const pa = parseSemver(a);
    const pb = parseSemver(b);
    if (!pa || !pb) return false;
    for (let i = 0; i < 3; i++) {
        if (pa[i] > pb[i]) return true;
        if (pa[i] < pb[i]) return false;
    }
    // Numeric parts equal — compare prerelease tags.
    //   * no prerelease > any prerelease       (1.2.3   > 1.2.3-beta)
    //   * same prerelease => equal             (1.2.3-x = 1.2.3-x)
    //   * different prereleases: conservatively NOT >= (safe default)
    const preA = pa[3];
    const preB = pb[3];
    if (preA === preB) return true;
    if (preA === "") return true;
    if (preB === "") return false;
    return false;
}

// ── Dependabot alerts ────────────────────────────────────────────────────

function fetchAlerts(repo) {
    // Local-test escape hatch: read alerts from a JSON file instead of
    // calling the GitHub API. Only ever set in local seed tests; in CI
    // this env var is unset and the real ``gh api`` path runs.
    const mockPath = process.env.DEP_MOCK_ALERTS_FILE;
    if (mockPath) {
        dbg(`[mock] reading alerts from ${mockPath}`);
        try {
            return JSON.parse(readFileSync(mockPath, "utf8"));
        } catch (e) {
            err(`Could not read mock alerts file ${mockPath}: ${e.message}`);
            process.exit(1);
        }
    }
    const r = run("gh", [
        "api",
        "--paginate",
        `repos/${repo}/dependabot/alerts?state=open&per_page=100`,
    ]);
    if (!r.ok) {
        err("Failed to fetch Dependabot alerts via gh CLI.");
        err(r.stderr.trim());
        process.exit(1);
    }
    // --paginate concatenates JSON arrays as ``][`` between pages.
    const raw = r.stdout.trim();
    if (!raw) return [];
    const joined = "[" + raw.replace(/\]\s*\[/g, ",").slice(1, -1) + "]";
    try {
        return JSON.parse(joined);
    } catch (e) {
        err("Could not parse gh paginated JSON:", e.message);
        process.exit(1);
    }
}

/**
 * Group raw alerts by package name. Returns
 * ``[{ pkg, minVersion, severity, ghsaIds, alerts }]``. ``minVersion`` is
 * the highest ``first_patched_version`` across this package's open alerts
 * — i.e. the lowest version that resolves *all* known advisories.
 */
function groupAlerts(alerts) {
    const groups = new Map();
    let skippedNonNuGet = 0;
    for (const a of alerts) {
        const eco = a.dependency?.package?.ecosystem;
        const pkg = a.dependency?.package?.name;
        if (eco !== "nuget" || !pkg) {
            skippedNonNuGet++;
            continue;
        }

        const patched =
            a.security_vulnerability?.first_patched_version?.identifier;
        const ghsaId = a.security_advisory?.ghsa_id;
        if (!groups.has(pkg)) {
            groups.set(pkg, {
                pkg,
                ecosystem: eco,
                minVersion: patched || null,
                severity: a.security_advisory?.severity || "unknown",
                ghsaIds: new Set(),
                alerts: [],
            });
        }
        const g = groups.get(pkg);
        g.alerts.push(a);
        if (ghsaId) g.ghsaIds.add(ghsaId);
        if (patched && (!g.minVersion || semverGte(patched, g.minVersion))) {
            g.minVersion = patched;
        }
        const sevRank = { critical: 4, high: 3, medium: 2, low: 1, unknown: 0 };
        if (
            sevRank[a.security_advisory?.severity] >
            sevRank[g.severity]
        ) {
            g.severity = a.security_advisory.severity;
        }
    }
    if (skippedNonNuGet > 0) {
        dbg(`Ignored ${skippedNonNuGet} non-nuget or malformed alert(s)`);
    }
    return [...groups.values()];
}

// ── Directory.Packages.props editing ─────────────────────────────────────
//
// The file format is a fixed XML schema:
//   <Project>
//     <PropertyGroup> ... </PropertyGroup>
//     <ItemGroup>
//       <PackageVersion Include="X" Version="Y" />
//       ...
//     </ItemGroup>
//   </Project>
// We do a string-level edit rather than full XML parsing to preserve
// formatting, comments, and surrounding whitespace exactly.

function readPackagesProps() {
    const p = join(REPO_ROOT, PACKAGES_PROPS);
    if (!existsSync(p)) {
        err(`Missing ${PACKAGES_PROPS} at repo root. This script requires Central Package Management.`);
        process.exit(1);
    }
    return readFileSync(p, "utf8");
}

function writePackagesProps(text) {
    writeFileSync(join(REPO_ROOT, PACKAGES_PROPS), text);
}

/**
 * Update an existing ``<PackageVersion Include="<pkg>" Version="..." />``
 * entry to the new version, or insert a new one (for transitive pins) if
 * the package isn't yet listed. Returns the new file text.
 */
function setPackageVersion(text, pkg, newVersion) {
    // Build an Include-attribute matcher. Names are case-insensitive in
    // NuGet but case-preserving in MSBuild XML, so match
    // case-insensitively and preserve the existing casing.
    const escaped = pkg.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    const re = new RegExp(
        `(<PackageVersion\\s+Include="${escaped}"\\s+Version=")([^"]+)("\\s*/>)`,
        "i",
    );
    if (re.test(text)) {
        return text.replace(re, `$1${newVersion}$3`);
    }
    // Insert a new transitive pin just before the closing </ItemGroup>.
    // For multi-ItemGroup props we target the LAST one (where the bulk
    // of <PackageVersion> entries live in convention).
    const lastClose = text.lastIndexOf("</ItemGroup>");
    if (lastClose < 0) {
        throw new Error(
            `Cannot find </ItemGroup> in ${PACKAGES_PROPS} to insert transitive pin for ${pkg}`,
        );
    }
    // Discover the indentation used inside the ItemGroup by looking at
    // the most recent <PackageVersion line.
    const upTo = text.slice(0, lastClose);
    const indentMatch = upTo.match(/(\r?\n)([ \t]+)<PackageVersion\b/);
    const eol = indentMatch ? indentMatch[1] : "\n";
    const indent = indentMatch ? indentMatch[2] : "    ";
    // Walk back over any whitespace immediately preceding </ItemGroup> so
    // we insert at the start of its line — otherwise that leading
    // whitespace becomes a prefix on our inserted comment line.
    let insertAt = lastClose;
    while (insertAt > 0 && (text[insertAt - 1] === " " || text[insertAt - 1] === "\t")) {
        insertAt--;
    }
    const insertion = `${indent}<!-- Transitive pin added by fix-dependabot-alerts -->${eol}${indent}<PackageVersion Include="${pkg}" Version="${newVersion}" />${eol}${text.slice(insertAt, lastClose)}`;
    return text.slice(0, insertAt) + insertion + text.slice(lastClose);
}

function hasPackageEntry(text, pkg) {
    const escaped = pkg.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
    return new RegExp(
        `<PackageVersion\\s+Include="${escaped}"\\s+Version="`,
        "i",
    ).test(text);
}

// ── Backup / restore ─────────────────────────────────────────────────────

function backupProps() {
    const bak = join(RUN_TEMP, "tc-packages-props-backup.xml");
    copyFileSync(join(REPO_ROOT, PACKAGES_PROPS), bak);
    return bak;
}

function restoreProps(bak) {
    copyFileSync(bak, join(REPO_ROOT, PACKAGES_PROPS));
}

// ── Rollback state ───────────────────────────────────────────────────────

function loadRollbackState() {
    if (!existsSync(ROLLBACK_STATE_PATH)) {
        return { version: 1, rollbacks: {} };
    }
    try {
        return JSON.parse(readFileSync(ROLLBACK_STATE_PATH, "utf8"));
    } catch {
        return { version: 1, rollbacks: {} };
    }
}

function saveRollbackState(state) {
    writeFileSync(ROLLBACK_STATE_PATH, JSON.stringify(state, null, 2));
}

function propsSha() {
    const r = run("git", ["hash-object", join(REPO_ROOT, PACKAGES_PROPS)]);
    return r.ok ? r.stdout.trim() : "unknown";
}

function isRecentlyRolledBack(state, key, currentSha) {
    const e = state.rollbacks?.[key];
    if (!e) return null;
    if (e.propsSha !== currentSha) return null;
    const ageSec = Math.floor(Date.now() / 1000) - e.timestamp;
    if (ageSec > ROLLBACK_COOLDOWN_DAYS * 86400) return null;
    return { ageDays: Math.floor(ageSec / 86400), reason: e.reason };
}

function recordRollback(state, key, reason, currentSha) {
    state.rollbacks ||= {};
    state.rollbacks[key] = {
        propsSha: currentSha,
        timestamp: Math.floor(Date.now() / 1000),
        reason,
    };
}

function clearRollback(state, key) {
    if (state.rollbacks?.[key]) delete state.rollbacks[key];
}

function pruneRollbacks(state) {
    const cutoff =
        Math.floor(Date.now() / 1000) - ROLLBACK_COOLDOWN_DAYS * 86400;
    for (const [k, v] of Object.entries(state.rollbacks || {})) {
        if (v.timestamp < cutoff) delete state.rollbacks[k];
    }
}

// ── Restore + audit verification ─────────────────────────────────────────
//
// We run ``dotnet restore`` and parse the warnings stream for any
// NU1901/NU1902/NU1903/NU1904 message that references one of the
// alert's GHSA IDs. If none remain for this package, the fix is
// considered verified at the audit layer.

function restoreAndAudit(pkg, ghsaIds) {
    // Force restore to bypass the no-op short-circuit when MSBuild
    // thinks the project assets are already up to date — the props
    // file changed in-place and the input check doesn't catch it.
    const r = run("dotnet", ["restore", SOLUTION, "--force"], {
        cwd: REPO_ROOT,
    });
    const combined = (r.stdout || "") + "\n" + (r.stderr || "");
    if (!r.ok) {
        return {
            ok: false,
            reason: `dotnet restore failed: ${combined.split("\n").filter(Boolean).slice(-3).join(" | ")}`,
        };
    }
    // Each NU190x line includes the package name and the GHSA URL.
    // Example:
    //   warning NU1903: Package 'Microsoft.SemanticKernel' 1.68.0 has a known high severity vulnerability, https://github.com/advisories/GHSA-2ww3-72rp-wpp4
    const lines = combined.split(/\r?\n/);
    const remaining = lines.filter((l) => {
        if (!/warning NU190[1-4]\b/i.test(l)) return false;
        // Match by package name (more reliable than GHSA when the
        // alert's set of GHSA IDs is incomplete, but we also match by
        // GHSA to disambiguate when multiple advisories exist).
        if (!l.toLowerCase().includes(`'${pkg.toLowerCase()}'`)) return false;
        if (ghsaIds.size === 0) return true;
        for (const id of ghsaIds) {
            if (l.toLowerCase().includes(id.toLowerCase())) return true;
        }
        return false;
    });
    if (remaining.length > 0) {
        return {
            ok: false,
            reason: `audit still reports vulnerability: ${remaining[0].trim()}`,
        };
    }
    return { ok: true };
}

function buildAndTest() {
    const env = sanitizedEnv();
    // /warnaserror matches the CI build line. -m:1 avoids a known race
    // in the examples Directory.Build.targets file copy.
    const build = run(
        "dotnet",
        ["build", SOLUTION, "-c", "Release", "/warnaserror", "--no-restore", "-m:1"],
        { cwd: REPO_ROOT, env },
    );
    if (!build.ok) {
        return {
            ok: false,
            phase: "build",
            output: (build.stdout || "") + (build.stderr || ""),
        };
    }
    if (SKIP_TESTS) return { ok: true };
    const test = run(
        "dotnet",
        ["test", TEST_PROJECT, "-c", "Release", "--no-build", "--nologo"],
        { cwd: REPO_ROOT, env },
    );
    if (!test.ok) {
        return {
            ok: false,
            phase: "test",
            output: (test.stdout || "") + (test.stderr || ""),
        };
    }
    return { ok: true };
}

// ── Main fix loop ────────────────────────────────────────────────────────

function applyFix(group, state) {
    const { pkg, minVersion, severity, ghsaIds } = group;
    const key = pkg;
    const currentSha = propsSha();

    log("");
    log(
        `▶  ${pkg} (${severity}, ${group.alerts.length} alert${group.alerts.length === 1 ? "" : "s"})  →  ≥ ${minVersion || "?"}`,
    );

    if (!minVersion) {
        log(
            `   skipped: no first_patched_version on advisory (likely awaiting upstream fix)`,
        );
        return { status: "no_patch", pkg, severity };
    }

    const cooldown = isRecentlyRolledBack(state, key, currentSha);
    if (cooldown) {
        log(
            `   skipped: rolled back ${cooldown.ageDays}d ago against same Directory.Packages.props (reason: ${cooldown.reason})`,
        );
        return { status: "skipped_cooldown", pkg, severity };
    }

    if (DRY_RUN) {
        const text = readPackagesProps();
        const direct = hasPackageEntry(text, pkg);
        log(
            `   dry-run: would ${direct ? "bump existing" : "add transitive pin"} <PackageVersion Include="${pkg}" Version="${minVersion}" />`,
        );
        return { status: "would_fix", pkg, severity };
    }

    const backup = backupProps();
    const before = readFileSync(backup, "utf8");
    const direct = hasPackageEntry(before, pkg);
    const method = direct ? "version_bump" : "transitive_pin";

    let next;
    try {
        next = setPackageVersion(before, pkg, minVersion);
    } catch (e) {
        log(`   ${method}: ${e.message}`);
        return { status: "unfixable", pkg, severity, reason: e.message };
    }
    if (next === before) {
        log(`   ${method}: file unchanged (already at ${minVersion}?) — skipping`);
        return {
            status: "unfixable",
            pkg,
            severity,
            reason: "props file would be unchanged",
        };
    }
    writePackagesProps(next);

    const audit = restoreAndAudit(pkg, ghsaIds);
    if (!audit.ok) {
        log(`   ${method}: ${audit.reason}`);
        restoreProps(backup);
        return {
            status: "unfixable",
            pkg,
            severity,
            reason: `${method}: ${audit.reason}`,
        };
    }
    log(`   ${method}: restore clean, no remaining audit warnings for ${pkg}`);

    const v = buildAndTest();
    if (v.ok) {
        log(`   ✓ verified (restore + build${SKIP_TESTS ? "" : " + test"})`);
        clearRollback(state, key);
        return {
            status: "applied",
            pkg,
            severity,
            method,
            minVersion,
        };
    }

    // The fix took (audit clean), but the workspace no longer
    // builds/tests. Roll back and remember.
    log(`   ✗ ${v.phase} failed after ${method}; rolling back`);
    if (VERBOSE && v.output) {
        console.log(v.output.slice(0, 4000));
    }
    restoreProps(backup);
    recordRollback(state, key, `${v.phase} failed (${method})`, currentSha);
    return { status: "rolled_back", pkg, severity, phase: v.phase };
}

// ── Reporting ────────────────────────────────────────────────────────────

function bucket(results) {
    const b = {
        applied: [],
        rolled_back: [],
        unfixable: [],
        no_patch: [],
        skipped_cooldown: [],
        would_fix: [],
    };
    for (const r of results) (b[r.status] ||= []).push(r);
    return b;
}

function printSummary(b, totals) {
    log("");
    log("─── Summary ───────────────────────────────────────────────");
    log(`  Total alerts:     ${totals.alerts}`);
    log(`  Distinct pkgs:    ${totals.packages}`);
    log(`  Applied:          ${b.applied.length}${b.applied.length ? " — " + b.applied.map((r) => `${r.pkg}(${r.method})`).join(" ") : ""}`);
    log(`  Rolled back:      ${b.rolled_back.length}${b.rolled_back.length ? " — " + b.rolled_back.map((r) => `${r.pkg}(${r.phase})`).join(" ") : ""}`);
    log(`  Unfixable:        ${b.unfixable.length}${b.unfixable.length ? " — " + b.unfixable.map((r) => r.pkg).join(" ") : ""}`);
    log(`  No patch yet:     ${b.no_patch.length}${b.no_patch.length ? " — " + b.no_patch.map((r) => r.pkg).join(" ") : ""}`);
    log(`  Cooldown skipped: ${b.skipped_cooldown.length}${b.skipped_cooldown.length ? " — " + b.skipped_cooldown.map((r) => r.pkg).join(" ") : ""}`);
    if (DRY_RUN) {
        log(`  Would attempt:    ${b.would_fix.length}${b.would_fix.length ? " — " + b.would_fix.map((r) => r.pkg).join(" ") : ""}`);
    }
}

function writeStepOutputs(b, totals) {
    const out = process.env.GITHUB_OUTPUT;
    const transitivePins = b.applied
        .filter((r) => r.method === "transitive_pin")
        .map((r) => r.pkg)
        .join(" ");
    const lines = [
        `total_alerts=${totals.alerts}`,
        `applied_count=${b.applied.length}`,
        `applied_packages=${b.applied.map((r) => r.pkg).join(" ")}`,
        `applied_transitive_pins=${transitivePins}`,
        `rolled_back_count=${b.rolled_back.length}`,
        `rolled_back_packages=${b.rolled_back.map((r) => r.pkg).join(" ")}`,
        `unfixable_count=${b.unfixable.length}`,
        `unfixable_packages=${b.unfixable.map((r) => r.pkg).join(" ")}`,
        `no_patch_count=${b.no_patch.length}`,
        `no_patch_packages=${b.no_patch.map((r) => r.pkg).join(" ")}`,
        `cooldown_count=${b.skipped_cooldown.length}`,
        `cooldown_packages=${b.skipped_cooldown.map((r) => r.pkg).join(" ")}`,
        `changes=${b.applied.length > 0 ? "true" : "false"}`,
    ];
    if (out) {
        writeFileSync(out, lines.join("\n") + "\n", { flag: "a" });
    } else if (VERBOSE) {
        log("");
        log("--- step outputs ---");
        for (const l of lines) log(l);
    }
}

// ── Entry point ──────────────────────────────────────────────────────────

function main() {
    const repo =
        process.env.GITHUB_REPOSITORY ||
        process.env.DEP_REPO ||
        "microsoft/typechat.net";

    log(`Repo:       ${repo}`);
    log(`Mode:       ${DRY_RUN ? "analyze (dry-run)" : "auto-fix"}`);
    log(`State file: ${ROLLBACK_STATE_PATH}`);

    const alerts = fetchAlerts(repo);
    log(`Fetched ${alerts.length} open alerts`);

    const groups = groupAlerts(alerts);
    log(`Grouped into ${groups.length} distinct package(s)`);

    const state = loadRollbackState();
    pruneRollbacks(state);

    const results = [];
    for (const g of groups) {
        results.push(applyFix(g, state));
    }

    if (!DRY_RUN) {
        saveRollbackState(state);
    }

    const b = bucket(results);
    printSummary(b, { alerts: alerts.length, packages: groups.length });
    writeStepOutputs(b, { alerts: alerts.length, packages: groups.length });

    if (b.rolled_back.length > 0) {
        warn(
            `${b.rolled_back.length} package(s) rolled back; their alerts remain open.`,
        );
    }
    if (b.unfixable.length > 0) {
        warn(
            `${b.unfixable.length} package(s) could not be lifted to a safe version (likely the advisory's first_patched_version doesn't yet exist in NuGet, or the upgrade pulls in incompatible transitive constraints). Their alerts remain open.`,
        );
    }
}

main();
