using Conversation.Web.Services;

namespace Conversation.Web.GraphQL;

public class Query
{
    public IReadOnlyList<Models.Conversation> GetConversations(
        [Service] IConversationService service)
        => service.GetConversations();

    public Models.Conversation? GetConversation(
        string id,
        [Service] IConversationService service)
        => service.GetConversation(id);

    public IReadOnlyList<Models.Message> GetMessages(
        string conversationId,
        [Service] IConversationService service)
        => service.GetMessages(conversationId);

    public Models.Message? GetMessage(
        string id,
        [Service] IConversationService service)
        => service.GetMessage(id);
}
