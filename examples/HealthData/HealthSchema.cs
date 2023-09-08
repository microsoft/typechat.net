﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using Microsoft.TypeChat.Schema;
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
    public string DisplayText { get; set; }

    [Comment("If precise quantity can be set")]
    public Quantity? Quantity { get; set; }
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

public class MedicationResponse : AgentResponse<Medication>
{

}

public static class HealthVocabs
{
    public const string DoseForm = "doseForm";
}
