# About TypeChat

**TypeChat** is a library that makes it easy to build natural language interfaces using types.

Building natural language interfaces has traditionally been difficult. These apps often
relied on complex decision trees to determine intent and collect the required inputs to take action.
Large language models (LLMs) have made this easier by enabling us to take natural language input
from a user and match to intent. This has introduced its own challenges including the need to
constrain the model's reply for safety, structure responses from the model for further processing,
and ensuring that the reply from the model is valid. Prompt engineering aims to solve these problems,
but comes with a steep learning curve and increased fragility as the prompt increases in size.

TypeChat replaces prompt engineering with schema engineering.

Simply define types that represent the intents supported in your natural language application. That could be as simple as an interface for categorizing sentiment or more complex examples like types for a shopping cart or music application. For example, to add additional intents to a schema, a developer can add additional types into a discriminated union. To make schemas hierarchical, a developer can use a "meta-schema" to choose one or more sub-schemas based on user input.

After defining your types, TypeChat takes care of the rest by:

Constructing a prompt to the LLM using types.
Validating the LLM response conforms to the schema. If the validation fails, repair the non-conforming output through further language model interaction.
Summarizing succinctly (without use of a LLM) the instance and confirm that it aligns with user intent.
Types are all you need!

# Getting Started ⚡

- Check out the [GitHub repository](https://github.com/microsoft/typechat.net) for the latest updates.
