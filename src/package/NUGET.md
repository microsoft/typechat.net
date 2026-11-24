# TypeChat.NET

[TypeChat.NET](https://github.com/microsoft/typechat.net) brings the ideas of [TypeChat](https://github.com/microsoft/TypeChat) to .NET. 

TypeChat.NET provides **cross platform** libraries that help you build natural language interfaces with language models using strong types and type validation. TypeChat provides:
* Json Translation and validation.
* Schema export: generate schema for .NET Types using **Typescript**, a language designed to express the schema of JSON objects concisely. Schema export supports common scenarios (as shown in the examples) and provides:
  * Runtime export: This is needed for scenarios where the schema may vary for each request, may have to be selected from sub-schemas or include dynamic lists and vocabularies (such as product names, or lists of players in a team).
  * Vocabularies: easy specification of string tables along with support for dynamic loading. 
  
* Extensibility: interfaces let you customize schemas (including hand-written), validators and even language model prompts.

# Getting Started

In the 0.3 preview, chat translation puts the application schema and JSON format in a system
message, then preserves preamble and request sections in order. A fresh user request is sent
verbatim. Use a role-aware chat adapter, or explicitly select `JsonTranslatorPrompts.Legacy`
for the previous text layout. See the [prompt migration guide](https://github.com/microsoft/typechat.net#chat-prompts-in-03-preview).

Learn about and get the latest updates on the TypeChat.NET project on the [TypeChat.NET GitHub repository](https://github.com/microsoft/typechat.net).