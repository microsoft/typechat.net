# CoffeeShop
Demonstrates **JsonTranslator** with a [complex schema](CoffeeShopSchema.cs) for a coffee shop. It shows how GPT 3.5 and 4.0 can effectively be used to translate a broad range of user intent into strongly typed objects that can then be processed with conventional software.

CoffeeShop Emulates natural language ordering at a Coffee Shop that serves:
- Coffee
- Espresso
- Lattes
- Baked Good

A range of syrups and other familiar options are also offered. 

Also demonstrates how to use the ***[JsonVocab]*** attribute to:
* Specify string vocabularies like:
  ```[JsonVocab(whole milk | two percent milk | nonfat milk | soy milk | almond milk | oat milk)]```
* Automatic validation and repair of strings using these vocabularies

# Usage

Example prompts can be found in [`input.txt`](input.txt).

**Input**:

```
we'd like a cappuccino with a pack of sugar, 1 tall latte with extra foam, 1 latte with vanilla syrup and 3 blueberry muffins.
```
