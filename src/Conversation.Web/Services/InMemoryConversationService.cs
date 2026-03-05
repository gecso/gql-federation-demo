using System.Collections.Concurrent;
using Conversation.Web.Models;

namespace Conversation.Web.Services;

public class InMemoryConversationService : IConversationService
{
    private readonly ConcurrentDictionary<string, Models.Conversation> _conversations = new();
    private readonly ConcurrentDictionary<string, Message> _messages = new();

    public IReadOnlyList<Models.Conversation> GetConversations()
        => _conversations.Values.OrderBy(c => c.CreatedAt).ToList();

    public Models.Conversation? GetConversation(string id)
        => _conversations.TryGetValue(id, out var conv) ? conv : null;

    public IReadOnlyList<Message> GetMessages(string conversationId)
        => _messages.Values
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.SentAt)
            .ToList();

    public Message? GetMessage(string id)
        => _messages.TryGetValue(id, out var msg) ? msg : null;

    public Models.Conversation CreateConversation(string title)
    {
        var conv = new Models.Conversation
        {
            Id = Guid.NewGuid().ToString(),
            Title = title,
            CreatedAt = DateTime.UtcNow
        };
        _conversations[conv.Id] = conv;
        return conv;
    }

    public Models.Conversation UpdateConversation(string id, string title)
    {
        if (!_conversations.TryGetValue(id, out var conv))
            throw new KeyNotFoundException($"Conversation '{id}' not found.");

        conv.Title = title;
        conv.UpdatedAt = DateTime.UtcNow;
        return conv;
    }

    public bool DeleteConversation(string id)
    {
        if (!_conversations.TryRemove(id, out _))
            return false;

        var messageIds = _messages.Values
            .Where(m => m.ConversationId == id)
            .Select(m => m.Id)
            .ToList();

        foreach (var msgId in messageIds)
            _messages.TryRemove(msgId, out _);

        return true;
    }

    public Message SendMessage(string conversationId, string content, string authorId)
    {
        if (!_conversations.ContainsKey(conversationId))
            throw new KeyNotFoundException($"Conversation '{conversationId}' not found.");

        var msg = new Message
        {
            Id = Guid.NewGuid().ToString(),
            ConversationId = conversationId,
            Content = content,
            AuthorId = authorId,
            SentAt = DateTime.UtcNow
        };
        _messages[msg.Id] = msg;
        _conversations[conversationId].Messages.Add(msg);
        return msg;
    }

    public bool DeleteMessage(string id)
    {
        if (!_messages.TryRemove(id, out var msg))
            return false;

        if (_conversations.TryGetValue(msg.ConversationId, out var conv))
            conv.Messages.RemoveAll(m => m.Id == id);

        return true;
    }
}
