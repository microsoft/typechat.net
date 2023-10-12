# CoffeeShop2

CoffeeShop2 implements the same features using the same schema as [CoffeeShop](../CoffeeShop/Readme.md) but with the following differences:
* Vocabularies are **dynamically loaded at runtime**. CoffeeShop2 demonstrates how to use [IVocab](../../src/typechat/Schema/IVocab.cs) and [IVocabCollection](../../src/typechat/Schema/IVocabCollection.cs). 
  * Vocab Strings are loaded from a file and bound dynamically. The same vocabularies could also be loaded from a database or any other source. This lets you alter vocabularies without recompiling your code.
  * Each request could use a different translator and associated vocabulary. 
    * E.g. Premium Coffee Shop users could be offered more complex Syrups or Toppings. Vegan users would only see non-dairy milks. Etc.
* Uses a ILanguageModel implemented with the [Microsoft SemanticKernel](https://github.com/microsoft/semantic-kernel)

## Target models
Works with gpt-35-turbo and gpt-4.

# Usage

Example prompts can be found in [`input.txt`](input.txt).

**Input**:

```
we'd like a cappuccino with a pack of sugar
```

