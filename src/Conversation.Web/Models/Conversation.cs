namespace Conversation.Web.Models;

public class Conversation
{
    public string Id { get; set; } = default!;
    public string Title { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<Message> Messages { get; set; } = new();
}
