# TypeChat.NET

TypeChat.NET makes it easy to build natural language interfaces using types.

It applies the ideas of [TypeChat](https://github.com/microsoft/TypeChat) to .NET and integrates them with the [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel). 

TypeChat.NET consists of two assemblies:
- typechat
- typechat.sk

## TypeChat ##
Brings ideas of TypeChat to .NET.
- Json Translators
- Json Validators

The library is a faithful port of the original Typescript library with .NET idiom introduced suitably. But it does not implement bindings to a specific AI model or API. For that you use the Typechat.sk library below, or roll your own.

## TypeChat.sk ##
TypeChat.SK makes it easy to get ***strong typed*** .NET objects from the [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel).

        using Microsoft.TypeChat.SemanticKernel;

        Schema schema = Schema.Load("./SentimentSchema.ts");
        var service = KernelFactory.JsonTranslator<SentimentResponse>(
            schema,
            "gpt-35-turbo",
            _config.OpenAI
        );
        SentimentResponse response = await service.TranslateAsync("Tonights gonna be a good night! A good good night!");

OR

        using Microsoft.TypeChat.SemanticKernel;

        // This will auto-generate a Typescript for the .NET type
        var service = KernelFactory.JsonTranslator<SentimentResponse>(
            "gpt-35-turbo",
            _config.OpenAI
        );
        SentimentResponse response = await service.TranslateAsync("Tonights gonna be a good night! A good good night!");


The library provides:
- LLM bings for TypeChat using the Semantic Kernel.
- An experimental .NET Types to Typescript schema exporter. 

## Building ##
You will need Visual Studio (VS Code is not tested). 
- Load ./typechat.sln into Visual Studio 2022
- Restore packages
- Build
- 

## Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
