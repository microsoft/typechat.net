
namespace Microsoft.TypeChat;

public interface ITypeChatPrompts
{
    string CreateRequestPrompt(string request);
    string CreateRepairPrompt(string validationError);

}
