namespace Conversation.Web.Models;

public sealed class Message
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");

    public Conversation? Conversation { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Text { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;
}
