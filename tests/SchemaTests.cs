// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class SchemaTests : TypeChatTest
{
    [Fact]
    public void ExportText()
    {
        TypeSchema schema = TypescriptExporter.GenerateSchema(typeof(Order));
        Assert.NotNull(schema);
        Assert.Equal(typeof(Order), schema.Type);
        Assert.False(string.IsNullOrEmpty(schema.Schema));
    }
}

