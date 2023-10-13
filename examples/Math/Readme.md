# Math

The Math example shows how to use **TypeChat.Program** for program generation based on an API schema. This example translates calculations into simple programs given an [`API`](MathAPI.cs) type that can perform the basic mathematical operations.

## Target models
Works with gpt-35-turbo and gpt-4.

# Usage

Example prompts can be found in [`input.txt`](input.txt).

For example, we could use natural language to describe mathematical operations, and TypeChat.Program will generate a json program for the math API defined in the schema. It will then execute the program.

**Input**:

```
multiply two by three, then multiply four by five, then sum the results
```
