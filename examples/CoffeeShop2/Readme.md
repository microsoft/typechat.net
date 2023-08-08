# CoffeeShop2

CoffeeShop2 implements CoffeeShop, but with dynamic vocabularies - how to use IVocabCollection and IVocab. 
- Vocab Strings are loaded from a file and bound dynamically. 
- Your vocabularies could be loaded from a database or any other source. This lets you alter vocabularies WITHOUT recompiling your code.  
- Each request could use a different translator and associated vocabulary (with associated schema generated on the fly). 
-- e.g. Premium Coffee Shop users could be offered more complex Syrups or Toppings. Vegan users would only see non-dairy milks. Etc.

# Usage

Example prompts can be found in [`input.txt`](input.txt).

**Input**:

```
we'd like a cappuccino with a pack of sugar
```

