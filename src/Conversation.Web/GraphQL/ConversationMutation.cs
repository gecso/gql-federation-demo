using ConversationModel = Conversation.Web.Models.Conversation;
using MessageModel = Conversation.Web.Models.Message;
using Conversation.Web.Storage;

namespace Conversation.Web.GraphQL;

public sealed class ConversationMutation
{
    public ConversationModel CreateConversation(string projectId, string topic, [Service] ConversationStore store) =>
        store.CreateConversation(projectId, topic);

    public MessageModel AddMessage(
        string conversationId,
        string text,
        string author,
        [Service] ConversationStore conversationStore,
        [Service] MessageStore messageStore)
    {
        if (!conversationStore.Exists(conversationId))
        {
            throw new InvalidOperationException($"Conversation with id '{conversationId}' not found.");
        }

        return messageStore.AddMessage(conversationId, text, author);
    }
}
