using ConversationModel = Conversation.Web.Models.Conversation;

namespace Conversation.Web.Storage;

public sealed class ConversationStore
{
    private readonly object _sync = new();

    private readonly List<ConversationRow> _conversations = [];

    public ConversationStore()
    {
        _conversations.Add(
            new ConversationRow(
                Id: "c-1",
                ProjectId: "p-1",
                CreatedAt: DateTime.UtcNow,
                Topic: "Kickoff"));
    }

    public IReadOnlyList<ConversationModel> GetAll(string? projectId, string? topic)
    {
        var pid = (projectId ?? string.Empty).Trim();
        var topicTerm = (topic ?? string.Empty).Trim();

        lock (_sync)
        {
            IEnumerable<ConversationRow> query = _conversations;

            if (pid.Length > 0)
            {
                query = query.Where(x => x.ProjectId.Equals(pid, StringComparison.OrdinalIgnoreCase));
            }

            if (topicTerm.Length > 0)
            {
                query = query.Where(x => x.Topic.Contains(topicTerm, StringComparison.OrdinalIgnoreCase));
            }

            return query.Select(MapConversation).ToArray();
        }
    }

    public ConversationModel? GetById(string id)
    {
        lock (_sync)
        {
            var conversation = _conversations.FirstOrDefault(x => x.Id == id);
            return conversation is null ? null : MapConversation(conversation);
        }
    }

    public bool Exists(string id)
    {
        lock (_sync)
        {
            return _conversations.Any(x => x.Id == id);
        }
    }

    public ConversationModel CreateConversation(string projectId, string topic)
    {
        var conversation = new ConversationRow(
            Id: Guid.NewGuid().ToString("N"),
            ProjectId: projectId,
            CreatedAt: DateTime.UtcNow,
            Topic: topic);

        lock (_sync)
        {
            _conversations.Add(conversation);
            return MapConversation(conversation);
        }
    }

    private ConversationModel MapConversation(ConversationRow source)
    {
        return new ConversationModel
        {
            Id = source.Id,
            ProjectId = source.ProjectId,
            CreatedAt = source.CreatedAt,
            Topic = source.Topic,
            Messages = []
        };
    }

    private sealed record ConversationRow(
        string Id,
        string ProjectId,
        DateTime CreatedAt,
        string Topic);
}
