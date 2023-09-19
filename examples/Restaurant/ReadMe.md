# Restaurant

The Restaurant example shows how to capture user intent as a set of "nouns", but with more complex linguistic input.
This example can act as a "stress test" for language models, illustrating the line between simpler and more advanced language models in handling compound sentences, distractions, and corrections.
This example also shows how we can create a "user intent summary" to display to a user.
It uses a natural language experience for placing an order with the [`Order`](RestaurantSchema.cs) type.

## Target models
Works with gpt-35-turbo and gpt-4.

# Usage

Example prompts can be found in [`input.txt`](input.txt).

For example, given the following order:

**Input**:

```
I want three pizzas, one with mushrooms and the other two with sausage. Make one sausage a small. And give me a whole Greek and a Pale Ale. And give me a Mack and Jacks.
```
