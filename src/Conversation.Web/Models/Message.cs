namespace Conversation.Web.Models;

public class Message
{
    public string Id { get; set; } = default!;
    public string ConversationId { get; set; } = default!;
    public string Content { get; set; } = default!;
    public string AuthorId { get; set; } = default!;
    public DateTime SentAt { get; set; }
}
