﻿# Schema Hierarchy

This application demonstrates how to implement hierarchical schema using an example [HierarchicalJsonTranslator](../../src/typechat.examplesLib/HierarchicalJsonTranslator.cs). 

The HierarchicalJsonTranslator uses embeddings to automatically picks the best child schema for a request using embedings. Sub-schemas are:
* CoffeeShop
* Restaurant
* Calendar
* Sentiment
* HealthDataResponse

## Target models
Works with gpt-35-turbo and gpt-4.

# Usage
Example prompts can be found in [`input.txt`](input.txt).
