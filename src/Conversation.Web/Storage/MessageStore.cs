using ConversationModel = Conversation.Web.Models.Conversation;
using MessageModel = Conversation.Web.Models.Message;

namespace Conversation.Web.Storage;

public sealed class MessageStore
{
    private readonly object _sync = new();

    private readonly List<MessageRow> _messages =
    [
        new(
            Id: "m-1",
            ConversationId: "c-1",
            CreatedAt: DateTime.UtcNow,
            Text: "Conversation created",
            Author: "system")
    ];

    public IReadOnlyList<MessageModel> GetByConversationId(string conversationId)
    {
        lock (_sync)
        {
            return _messages
                .Where(x => x.ConversationId == conversationId)
                .Select(MapMessage)
                .ToArray();
        }
    }

    public MessageModel AddMessage(string conversationId, string text, string author)
    {
        var row = new MessageRow(
            Id: Guid.NewGuid().ToString("N"),
            ConversationId: conversationId,
            CreatedAt: DateTime.UtcNow,
            Text: text,
            Author: author);

        lock (_sync)
        {
            _messages.Add(row);
            return MapMessage(row);
        }
    }

    private static MessageModel MapMessage(MessageRow source) =>
        new()
        {
            Id = source.Id,
            Conversation = new ConversationModel { Id = source.ConversationId },
            CreatedAt = source.CreatedAt,
            Text = source.Text,
            Author = source.Author
        };

    private sealed record MessageRow(
        string Id,
        string ConversationId,
        DateTime CreatedAt,
        string Text,
        string Author);
}
