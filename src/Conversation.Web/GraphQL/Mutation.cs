using Conversation.Web.Services;

namespace Conversation.Web.GraphQL;

public class Mutation
{
    public Models.Conversation CreateConversation(
        string title,
        [Service] IConversationService service)
        => service.CreateConversation(title);

    public Models.Conversation? UpdateConversation(
        string id,
        string title,
        [Service] IConversationService service)
    {
        try
        {
            return service.UpdateConversation(id, title);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public bool DeleteConversation(
        string id,
        [Service] IConversationService service)
        => service.DeleteConversation(id);

    public Models.Message? SendMessage(
        string conversationId,
        string content,
        string authorId,
        [Service] IConversationService service)
    {
        try
        {
            return service.SendMessage(conversationId, content, authorId);
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    public bool DeleteMessage(
        string id,
        [Service] IConversationService service)
        => service.DeleteMessage(id);
}
