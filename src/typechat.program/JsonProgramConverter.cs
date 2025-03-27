// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Internal: used to Deserialize json programs
/// </summary>
internal class JsonProgramConvertor : JsonConverter<Program>
{
    internal static readonly JsonSerializerOptions Options = JsonSerializerTypeValidator.DefaultOptions();

    private static readonly ProgramParser s_defaultParser = new ProgramParser();

    public override Program? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        JsonDocument doc = JsonDocument.ParseValue(ref reader);
        return s_defaultParser.Parse(doc);
    }

    public override void Write(Utf8JsonWriter writer, Program value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value.Source);
    }
}
