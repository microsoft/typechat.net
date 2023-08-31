// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class ChatTranslator<T> : JsonTranslator<T, Message<T>>
{
    IChatModel _model;

    public ChatTranslator(IChatModel model, SchemaText schema)
    : this(model, new JsonSerializerTypeValidator<T>(schema))
    {
    }

    public ChatTranslator(IChatModel model, IJsonTypeValidator<T> validator, IJsonTranslatorPrompts? prompts = null)
        : base(validator, prompts)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        _model = model;
    }

    protected override Task<string> GetResponseAsync(string prompt, IEnumerable<Message<T>>? context, RequestSettings? settings, CancellationToken cancelToken)
    {
        Message message = prompt;
        return _model.GetResponseAsync(message, context, settings, cancelToken);
    }
}
