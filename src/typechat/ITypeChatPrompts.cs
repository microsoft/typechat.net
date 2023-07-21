
namespace Microsoft.TypeChat;

public interface ITypeChatPrompts
{
    string CreateRequestPrompt(TypeSchema schema, string request);
    string CreateRepairPrompt(TypeSchema schema, string json, string validationError);
}
