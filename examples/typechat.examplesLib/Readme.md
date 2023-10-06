# Microsoft.TypeChat.ExamplesLib
TypeChat.ExamplesLib provides:
* Example classes that show how to use TypeChat for several use cases
* Extension methods and utilities used by TypeChat examples.
 
Example classes include:
* **HierarchicalJsonTranslator**: demonstrates how a  translator can use an in-memory vector index to semantically route request to child translators

* **PluginProgramTranslator**: Program translator that translates user requests into programs that call APIs defined by Microsoft Semantic Kernel Plugins

* **PluginProgramValidator**: Validates programs produced by PluginProgramTranslator

* **VectorizedMessageList**: Demonstrates how to use embeddings to select semantically relevant messages as ***context*** in Agent interactions

* **Config**: Simplifies loading configuration for examples from settings files.
