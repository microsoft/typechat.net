// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using Microsoft.TypeChat;
using Microsoft.TypeChat.Dialog;

namespace HealthData;

public class Quantity
{
    [Comment("Exact number")]
    public double Value { get; set; }
    [Comment("UNITS include mg, kg, cm, pounds, liter, ml, tablet, pill, cup, per-day, per-week..ETC")]
    public string Units { get; set; }
}

public class ApproxQuantity
{
    [JsonRequired]
    [Comment("Use if no precise quantity")]
    [Comment("Default: Unknown")]
    public string DisplayText { get; set; }

    [Comment("Optional: only if precise quantities are available")]
    public Quantity? Quantity { get; set; }
}

public class ApproxDateTime
{
    [JsonRequired]
    [Comment("Use if no precise date")]
    [Comment("Default: Unknown")]
    public string DisplayText { get; set; }

    [Comment("If precise timestamp can be set")]
    public DateTime? Timestamp { get; set; }
}

public class Medication
{
    public string Name { get; set; }

    [Comment("E.g. 2 tablets, 1 cup")]
    [Required]
    public ApproxQuantity Dose { get; set; }

    [Comment("E.g. twice a day, ")]
    [Required]
    public ApproxQuantity Frequency { get; set; }

    [Comment("E.g. 50 mg")]
    [Required]
    public ApproxQuantity Strength { get; set; }
}

[Comment("Disease, ailment, injury etc")]
public class Condition
{
    public string Name { get; set; }

    [JsonVocab("active | recurrence | relapse | inactive | remission | resolved | unknown")]
    public string Status { get; set; }

    [Required]
    public ApproxDateTime StartDate { get; set; }

    [Required]
    [Comment("If the disease ended")]
    public ApproxDateTime? EndDate { get; set; }
}

public class HealthData
{
    public Medication? Medication { get; set; }
    public Condition? Condition { get; set; }
}

public class HealthDataResponse
{
    [Comment("Use this to ask questions, respond to user, notify user")]
    public string Message { get; set; }

    [Comment("Return this if JSON has ALL required information. Else ask questions")]
    public HealthData? Data { get; set; }

    [Comment("Use this for text that was not understood")]
    public string[]? NotTranslated { get; set; }

    [JsonIgnore]
    public bool HasMessage => (!string.IsNullOrEmpty(Message));
}
