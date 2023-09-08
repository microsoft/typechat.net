// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json.Serialization;

namespace Library;

[JsonPolymorphic]
[JsonDerivedType(typeof(LibraryBook), typeDiscriminator: nameof(LibraryBook))]
[JsonDerivedType(typeof(UnknownBook), typeDiscriminator: nameof(UnknownBook))]
public class Book
{
    [JsonPropertyName("title")]
    public string Title { get; set; }
    [JsonPropertyName("author")]
    public string Author { get; set; }
    [JsonPropertyName("genre")]
    public Genre Genre { get; set; }
}

public class LibraryBook : Book
{
    [JsonPropertyName("isbn")]
    public string ISBN { get; set; }
}


public class UnknownBook : Book
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Genre
{
    Fiction,
    NonFiction,
    Fantasy,
    SciFi,
    Mystery,
    Romance,
    Thriller,
    Horror,
    Western,
    Biography,
    History,
    Poetry,
    Children,
    YoungAdult,
    Other
}
