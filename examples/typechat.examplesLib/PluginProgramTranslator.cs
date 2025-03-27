// Copyright (c) Microsoft. All rights reserved.

namespace Microsoft.TypeChat;

/// <summary>
/// Program translator that translates user requests into programs that call APIs defined by Microsoft Semantic Kernel Plugins
/// </summary>
public class PluginProgramTranslator
{
    private Kernel _kernel;
    private ProgramTranslator _translator;
    private PluginApi _pluginApi;
    private SchemaText _pluginSchema;

    /// <summary>
    /// Create a new translator that will produce programs that can call all skills and
    /// plugins registered with the given semantic kernel
    /// </summary>
    /// <param name="kernel"></param>
    /// <param name="model"></param>
    public PluginProgramTranslator(Kernel kernel, ModelInfo model)
    {
        _kernel = kernel;
        _pluginApi = new PluginApi(_kernel);
        _pluginSchema = _pluginApi.TypeInfo.ExportSchema(_pluginApi.TypeName);
        _translator = new ProgramTranslator(
            _kernel.ChatLanguageModel(model),
            new ProgramValidator(new PluginProgramValidator(_pluginApi.TypeInfo)),
            _pluginSchema
        );
    }

    /// <summary>
    /// Translator being used
    /// </summary>
    public ProgramTranslator Translator => _translator;

    /// <summary>
    /// Kernel this translator is working with
    /// </summary>
    public Kernel Kernel => _kernel;

    /// <summary>
    /// The "API" formed by the various plugins registered with the kernel 
    /// </summary>
    public PluginApi Api => _pluginApi;

    public SchemaText Schema => _pluginSchema;

    public Task<Program> TranslateAsync(string input, CancellationToken cancelToken)
    {
        return _translator.TranslateAsync(input, cancelToken);
    }
}
