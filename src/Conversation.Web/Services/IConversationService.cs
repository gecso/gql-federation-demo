using Conversation.Web.Models;

namespace Conversation.Web.Services;

public interface IConversationService
{
    IReadOnlyList<Models.Conversation> GetConversations();
    Models.Conversation? GetConversation(string id);
    IReadOnlyList<Message> GetMessages(string conversationId);
    Message? GetMessage(string id);
    Models.Conversation CreateConversation(string title);
    Models.Conversation UpdateConversation(string id, string title);
    bool DeleteConversation(string id);
    Message SendMessage(string conversationId, string content, string authorId);
    bool DeleteMessage(string id);
}
