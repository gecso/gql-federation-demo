using ConversationModel = Conversation.Web.Models.Conversation;
using Conversation.Web.Storage;
using HotChocolate;

namespace Conversation.Web.GraphQL;

[GraphQLName("Query")]
public sealed class ConversationQuery
{
    public IReadOnlyList<ConversationModel> GetConversations(
        string? projectId,
        string? topic,
        [Service] ConversationStore store,
        [Service] MessageStore messageStore) =>
        store.GetAll(projectId, topic)
            .Select(c => AttachMessages(c, messageStore))
            .ToArray();

    public ConversationModel? GetConversationById(
        string id,
        [Service] ConversationStore store,
        [Service] MessageStore messageStore)
    {
        var conversation = store.GetById(id);
        return conversation is null ? null : AttachMessages(conversation, messageStore);
    }

    private static ConversationModel AttachMessages(ConversationModel conversation, MessageStore messageStore)
    {
        conversation.Messages = messageStore.GetByConversationId(conversation.Id);
        return conversation;
    }
}
