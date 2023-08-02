// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

delegate T JsonParserFunc<T>(ref Utf8JsonReader reader);

internal static class JsonParser
{
    public static JsonElement GetStringProperty(this JsonElement elt, string propertyName)
    {
        if (!elt.TryGetProperty(propertyName, out JsonElement value))
        {
            JsonParser.Throw(JsonTokenType.PropertyName, propertyName);
        }
        value.EnsureIsType(JsonValueKind.String, propertyName);
        return value;
    }

    public static void EnsureIsType(this JsonElement elt, JsonValueKind kind, string? propertyName = null)
    {
        if (elt.ValueKind != kind)
        {
            JsonParser.Throw(kind, propertyName);
        }
    }

    public static void Throw(JsonValueKind expected, string? expectedName)
    {
        if (expectedName != null)
        {
            throw new JsonException($"Expected {expected} {expectedName}");
        }
        else
        {
            throw new JsonException($"Expected {expected}");
        }
    }

    public static void Throw(JsonTokenType expected)
    {
        throw new JsonException($"Expected {expected}");
    }

    public static void Throw(JsonTokenType expected, string expectedName)
    {
        throw new JsonException($"Expected {expected} {expectedName}");
    }
}
