// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

public class PluginProgramTranslator
{
    IKernel _kernel;
    ProgramTranslator _translator;
    PluginApi _pluginApi;
    SchemaText _pluginSchema;

    public PluginProgramTranslator(IKernel kernel, ModelInfo model)
    {
        _kernel = kernel;
        _pluginApi = new PluginApi(_kernel);
        _pluginSchema = _pluginApi.TypeInfo.ExportSchema(_pluginApi.TypeName);
        _translator = new ProgramTranslator(
            _kernel.LanguageModel(model),
            new ProgramValidator(new PluginProgramValidator(_pluginApi.TypeInfo)),
            _pluginSchema
        );
    }

    public ProgramTranslator Translator => _translator;
    public PluginApi Api => _pluginApi;
    public SchemaText Schema => _pluginSchema;

    public Task<Program> TranslateAsync(string input, CancellationToken cancelToken)
    {
        return _translator.TranslateAsync(input, cancelToken);
    }
}
