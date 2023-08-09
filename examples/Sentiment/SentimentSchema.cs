// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.TypeChat.Schema;

namespace Sentiment;

public class SentimentResponse
{
    [JsonPropertyName("sentiment")]
    [JsonVocab("negative | neutral | positive")]
    public string Sentiment { get; set; }
}
