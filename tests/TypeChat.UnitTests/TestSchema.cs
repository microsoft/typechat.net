// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat.Tests;

public class TestSchema : TypeChatTest
{
    [Fact]
    public void ExportBasic()
    {
        TypeSchema schema = TypescriptExporter.GenerateSchema(typeof(SentimentResponse));
        ValidateBasic(typeof(SentimentResponse), schema);
        Assert.True(schema.Schema.Text.Contains("sentiment"));

        schema = TypescriptExporter.GenerateSchema(typeof(Order), TestVocabs.All());
        ValidateBasic(typeof(Order), schema);

        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.ContainsSubstring("coffee", "CoffeeOrder[]"));
        Assert.True(lines.ContainsSubstring("desserts", "DessertOrder[]"));
        Assert.True(lines.ContainsSubstring("fruits", "FruitOrder[]"));
    }

    [Fact]
    public void ExportNullable()
    {
        var schema = TypescriptExporter.GenerateSchema(typeof(NullableTestObj));
        // Need better verifier
        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.ContainsSubstring("optionalText?", "string"));
        Assert.True(lines.ContainsSubstring("OptionalAmt?", "number"));

        VocabCollection vocabs = new VocabCollection { TestVocabs.Milks() };
        schema = TypescriptExporter.GenerateSchema(typeof(WrapperNullableObj), vocabs);
        lines = schema.Schema.Text.Lines();
        Assert.True(lines.ContainsSubstring("optionalText?", "string"));
        Assert.True(lines.ContainsSubstring("OptionalAmt?", "number"));
        Assert.True(lines.ContainsSubstring("milk?"));
    }

    [Fact]
    public void ExportMathAPI()
    {
        var schema = TypescriptExporter.GenerateAPI(typeof(IMathAPI));
        // Need better verifier
        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.ContainsSubstring("interface", "IMathAPI"));
        Assert.True(lines.ContainsSubstring("number", "add", "x", "y"));
    }

    [Fact]
    public void ExportStringAPI()
    {
        var schema = TypescriptExporter.GenerateAPI(typeof(IStringAPI));
        // Need better verifier
        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.ContainsSubstring("interface", "IStringAPI"));
        Assert.True(lines.ContainsSubstring("string", "concat", "args", "any"));
        Assert.True(lines.ContainsSubstring("string", "lowercase", "string"));
    }

    [Fact]
    public void ExportObjectAPI()
    {
        var schema = TypescriptExporter.GenerateAPI(typeof(IPersonApi));
        // Need better verifier
        var lines = schema.Schema.Text.Lines();
        Assert.True(lines.ContainsSubstring("interface", "IPersonApi"));
        Assert.True(lines.ContainsSubstring("toJsonArray", "Person[]"));
        Assert.True(lines.ContainsSubstring("interface", "Person"));
        Assert.True(lines.ContainsSubstring("interface", "Name"));
        Assert.True(lines.ContainsSubstring("interface", "Location"));
    }

    [Fact]
    public void ExportAsyncApi()
    {
        var schema = TypescriptExporter.GenerateAPI(typeof(IAsyncService));
        var lines = schema.Schema.Text.Lines();
        Assert.False(lines.ContainsSubstring("Task"));
        Assert.True(lines.ContainsSubstring("interface", "IAsyncService"));
        Assert.True(lines.ContainsSubstring("DoWork", "void"));
        Assert.True(lines.ContainsSubstring("Name", "GetNameOf"));
        Assert.True(lines.ContainsSubstring("interface", "Name"));
        Assert.False(lines.ContainsSubstring("interface", "string"));
    }

    [Fact]
    public void ExportGenerics()
    {
        var schema = TypescriptExporter.GenerateSchema(typeof(Parent<Name, Location>));
        var lines = schema.Schema.Text.Lines();
        // This is how generics are currently handled. This will be refined as we test out
        // how the AI does with generic type definitions.
        Assert.True(lines.ContainsSubstring("interface", "Parent_Name_Location"));
        Assert.True(lines.ContainsSubstring("Child_Name[]"));
        Assert.True(lines.ContainsSubstring("Child_Location[]"));
    }

    [Fact]
    public void ExportEnumerable_Issue218()
    {
        var schema = TypescriptExporter.GenerateSchema(typeof(Pizza));
        var lines = schema.Schema.Text.Lines();

        // IEnumerable<string> must be exported as a JSON array of strings...
        Assert.True(lines.ContainsSubstring("Toppings", "string[]"));
        Assert.True(lines.ContainsSubstring("Size", "string"));
        // ...and NOT as a leaky List_String interface exposing Capacity/Count/Item
        Assert.False(lines.ContainsSubstring("List_String"));
        Assert.False(lines.ContainsSubstring("Capacity"));
    }

    [Fact]
    public void ExportCollections()
    {
        var schema = TypescriptExporter.GenerateSchema(typeof(CollectionsObj));
        var lines = schema.Schema.Text.Lines();

        // IEnumerable<T>, List<T>, IList<T>, ICollection<T>, IReadOnlyList<T>, HashSet<T> and arrays => T[]
        Assert.True(lines.ContainsSubstring("Tags", "string[]"));
        Assert.True(lines.ContainsSubstring("Names", "Name[]"));
        Assert.True(lines.ContainsSubstring("Scores", "number[]"));
        Assert.True(lines.ContainsSubstring("Locations", "Location[]"));
        Assert.True(lines.ContainsSubstring("Ratings", "number[]"));
        Assert.True(lines.ContainsSubstring("UniqueTags", "string[]"));
        Assert.True(lines.ContainsSubstring("Aliases", "string[]"));

        // Dictionaries => Record<Key, Value>, including a dictionary whose value is itself a collection
        Assert.True(lines.ContainsSubstring("Counts", "Record<string, number>"));
        Assert.True(lines.ContainsSubstring("LocationsByCity", "Record<string, Location>"));
        Assert.True(lines.ContainsSubstring("NamesById", "Record<string, Name>"));
        Assert.True(lines.ContainsSubstring("Buckets", "Record<string, number[]>"));

        // Element/value types are still exported as interfaces
        Assert.True(lines.ContainsSubstring("interface", "Name"));
        Assert.True(lines.ContainsSubstring("interface", "Location"));

        // The internals of List<T>/Dictionary<> must NOT leak into the schema
        Assert.False(lines.ContainsSubstring("Capacity"));
        Assert.False(lines.ContainsSubstring("KeyValuePair"));
        Assert.False(lines.ContainsSubstring("interface", "List_"));
    }

    [Fact]
    public void SelfReferentialEnumerable_Terminates()
    {
        // Composite : IEnumerable<Composite> has no finite "array of..." representation.
        // Schema generation must terminate rather than loop forever while unwrapping it.
        TypescriptSchema schema = null;
        var worker = new System.Threading.Thread(
            () => schema = TypescriptExporter.GenerateSchema(typeof(CompositeHolder)))
        {
            IsBackground = true
        };
        worker.Start();
        bool finished = worker.Join(System.TimeSpan.FromSeconds(5));

        Assert.True(finished, "Schema generation did not terminate for a self-referential IEnumerable type");

        var lines = schema.Schema.Text.Lines();
        // A List<Composite> is still a normal array...
        Assert.True(lines.ContainsSubstring("Nodes", "Composite[]"));
        // ...but the self-enumerating Composite itself falls back to a plain object interface
        Assert.True(lines.ContainsSubstring("interface", "Composite"));
        Assert.True(lines.ContainsSubstring("Name", "string"));
    }
}

