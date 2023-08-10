// Copyright (c) Microsoft. All rights reserved.

using System.Reflection;
using System.Text.Json;

namespace Microsoft.TypeChat.Tests;

public class ProgramTest : TypeChatTest
{
    public static JsonDocument LoadMathPrograms()
    {
        string json = File.ReadAllText("mathPrograms.json");
        return Json.Parse<JsonDocument>(json);
    }

    public static JsonDocument LoadStringPrograms()
    {
        string json = File.ReadAllText("stringPrograms.json");
        return Json.Parse<JsonDocument>(json);
    }

    public static IEnumerable<object[]> GetMathPrograms()
    {
        JsonDocument doc = LoadMathPrograms();
        return GetPrograms(doc);
    }

    public static IEnumerable<object[]> GetStringPrograms()
    {
        JsonDocument doc = LoadStringPrograms();
        return GetPrograms(doc);
    }

    static IEnumerable<object[]> GetPrograms(params JsonDocument[] docs)
    {
        foreach (var doc in docs)
        {
            foreach (var obj in doc.RootElement.EnumerateObject())
            {
                var valueProp = obj.Value.GetProperty("result");
                object result = valueProp.ValueKind == JsonValueKind.Number ?
                                valueProp.GetDouble() :
                                valueProp.GetString();

                var program = obj.Value.GetProperty("source");
                yield return new object[] { program.ToString(), result };
            }
        }
    }
}
