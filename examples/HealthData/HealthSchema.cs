// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat.Schema;
using Microsoft.TypeChat.Dialog;

namespace HealthData;

public class Quantity
{
    public double Value { get; set; }
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

    [Comment("E.g. 2 tablets, 1 cup, etc")]
    [Comment("Required")]
    public ApproxQuantity Dosage { get; set; }

    [Comment("E.g. 50 mg")]
    [Comment("Required")]
    public ApproxQuantity Strength { get; set; }
}

public class HealthDataResponse : AgentResponse<Medication>
{

}

public static class HealthVocabs
{
    public const string DoseForm = "doseForm";
}
