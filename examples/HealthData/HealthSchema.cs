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
    public string Text { get; set; }
    public Quantity Quantity { get; set; }
}

public class Medication
{
    public string Name { get; set; }
    public Quantity Dose { get; set; }
    public Quantity Strength { get; set; }
}

public class HealthResponse : AgentResponse<Medication>
{

}

public static class HealthVocabs
{
    public const string DoseForm = "doseForm";
}
