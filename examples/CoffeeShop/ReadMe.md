# CoffeeShop
CoffeeShop demonstrates how to use **JsonTranslator** to create a natural language interface to take orders for a coffee shop. This requires a [complex schema](CoffeeShopSchema.cs). CoffeeShop shows how GPT 3.5 and 4.0 can effectively be used to translate a broad range of user intent into strongly typed objects that can then be processed with conventional software.

CoffeeShop emulates natural language ordering at a Coffee Shop serving:
* Coffee
* Espresso
* Lattes
* Baked Good

A range of syrups and other familiar options are also offered. 

CoffeeShop also demonstrates how to use the ***JsonVocab*** attribute to:
* Specify string vocabularies like:
  ```[JsonVocab(whole milk | two percent milk | nonfat milk | soy milk | almond milk | oat milk)]```
  * Vocabularies differ from enums because they can also be loaded on demand and/or be customized for each request (based on the request's context)
* Automatic validation and repair of strings using these vocabularies.

## Target models
Works with gpt-35-turbo and gpt-4.

# Usage

Example prompts can be found in [`input.txt`](input.txt).

**Input**:

```
we'd like a cappuccino with a pack of sugar, 1 tall latte with extra foam, 1 latte with vanilla syrup and 3 blueberry muffins.
```
