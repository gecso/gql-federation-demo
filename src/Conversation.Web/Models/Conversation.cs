namespace Conversation.Web.Models;

public sealed class Conversation
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public string ProjectId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Topic { get; set; } = string.Empty;

    public IReadOnlyList<Message> Messages { get; set; } = [];
}
