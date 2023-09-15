# TypeChat.NET

TypeChat.NET is an experimental project from the [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel) team. TypeChat.NET brings the ideas of [TypeChat](https://github.com/microsoft/TypeChat) to .NET. 

TypeChat.NET helps you build natural language interfaces with LLMs using strong types and type-safe programs. 

        // Translates user intent into strongly typed Calendar Actions
        var translator = new JsonTranslator<CalendarActions>(
            new LanguageModel(Config.LoadOpenAI())
        );

**Goal**: develop frameworks that enable strongly typed programming with AI. Currently supported scenarios are shown in the examples. TypeChat.NET is in **active and rapid development** with frequent updates and refactoring. When in doubt, look at the code. Comments and documentation will improve as the code settles. 


# Assemblies
TypeChat.NET consists of the following assemblies:
* **TypeChat**: Classes (JsonTranslator<T>) that translate user intent into strongly typed and validated objects. Additionally includes:
  * Support for automatically exporting .NET type information as schema expressed using the concise syntax of Typescript. This schema is sent to language models. 
  * Validation of returned JSON. 

* **TypeChat.Program**: Classes to synthesize, validate and run  ***JSON programs*** 

* **TypeChat.SemanticKernel**: Integration with Microsoft Semantic Kernel - access to language models, support for Semantic Kernel Plugins, Embeddings and Semantic Memory

* **TypeChat.Dialog** (Early): Classes for working with interactive Agents

* **TypeChat.App**: Useful support classes and extensions used by Typechat examples, such as Text Classifiers. These may also be useful for other apps built using Typechat


## TypeChat ##
Brings TypeChat to .NET with .NET idiom introduced as appropriate.
- Json Translators
- Json Validators
- Schema: exporters for .NET Types to schema expressed using Typescript. 

Schema export includes support for:
* Dynamic export at runtime, including with ***each request*** to the AI. This is needed for scenarios where the schema must include dynamic lists, such as relevant product names or lists of players in a team.
* Vocabularies: easy unions of string tables, like in Typescript, along with support for dynamic loading. See examples: CoffeeShop and CoffeeShop2.

## TypeChat.Program ##
TypeChat.Program translates natural language requests into simple programs (***Plans***), represented as JSON. These programs are then type checked, compiled and run with type safety.
TypeChat.Program includes:
- Program Translator: translates user intent into programs that follow the [Program Grammar](src/typechat.program/ProgramSchema.ts)
- Program Interpreter: Executes programs generated by ProgramTranslator using an interpreter.
- Program Compiler: uses the dynamic language runtime (DLR) to compile programs/plans into verifiable typesafe code that can be checked for errors... and ***repaired***. 
- Program C# Transpiler/Compiler (experimental): Transpile programs into C# and compile them into typesafe assemblies with Roslyn. Compilation diagnostics can be used to repair programs.  

        // Translates user intent into typed Programs that call
        // methods on a Math API
        _api = new MathAPI();
        _translator = new ProgramTranslator<IMathAPI>(
            new LanguageModel(Config.LoadOpenAI()),
            _api
        );
 
## TypeChat.SemanticKernel ##

The library contains classes for:
* LLM bindings for TypeChat using the Semantic Kernel. All TypeChat examples use the Semantic Kernel to call LLMs
* **Program synthesis with Plugins**: Automatically turns registered plugins into a PluginAPI that programs synthesized by the LLM can call. [Plugins Example](examples/Plugins/Program.cs)
 
# TypeChat.Dialog
TypeChat.Dialog is an early version of framework desiged for strongly typed interactions with Agents with built in interaction history and other features. 

    // Create an agent that interactively helps the user enter their health information, such as medications and conditions
    new Agent<HealthDataResponse>(new LanguageModel(Config.LoadOpenAI()))

# TypeChat.App
Helper classes for:
* Console Apps
* Text Classification
* Intent dispatch and routing
* Generation and validation of programs that use Plugins

# Getting Started 
## Building
You will need Visual Studio 2022. VS Code is not tested. 
* Load **typechat.sln** from the root directory of your . 
* Restore packages
* Build

## Examples

To see TypeChat in action, we recommend exploring the [TypeChat example projects](./examples). 

We sugggest exploring examples in this order:
* To learn about Typechat and JsonTranslator: 
  * Sentiment: the simplest example that demonstrates JsonTranslator and other core features 
  * CoffeeShop
  * Calendar
* To learn about TypeChat.Program and program synthesis
  * Math
  * Plugins (program synthesis that target Semantic Kernel Plugins)

### Api Key
- You will need to provide an **Open API key**
- Go to the ***examples*** folder in the solution
- Create appSettings.Development.json
- Add your Api Key

### Inputs
- Each example includes an **input.txt** with sample input. 
- Pass the input file as an argument to run the example in **batch mode**. 

# Contributing

This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

# Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft 
trademarks or logos is subject to and must follow 
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
