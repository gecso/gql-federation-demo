using Conversation.Web.GraphQL;
using Conversation.Web.Storage;

namespace Conversation.Web.Tests;

public class ConversationGraphQlTests
{
    // ── Queries ──────────────────────────────────────────────────────────────

    [Fact]
    public void GetConversations_ReturnsSeedConversation()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var query = new ConversationQuery();

        var conversations = query.GetConversations(null, null, conversationStore, messageStore);

        Assert.Single(conversations);
        Assert.Equal("c-1", conversations[0].Id);
        Assert.Equal("p-1", conversations[0].ProjectId);
        Assert.Equal("Kickoff", conversations[0].Topic);
    }

    [Fact]
    public void GetConversations_AttachesSeedMessages()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var query = new ConversationQuery();

        var conversations = query.GetConversations(null, null, conversationStore, messageStore);

        var messages = conversations[0].Messages;
        Assert.Single(messages);
        Assert.Equal("m-1", messages[0].Id);
        Assert.Equal("Conversation created", messages[0].Text);
        Assert.Equal("system", messages[0].Author);
    }

    [Fact]
    public void GetConversations_FiltersByProjectId()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var query = new ConversationQuery();
        var mutation = new ConversationMutation();

        mutation.CreateConversation("p-2", "Planning", conversationStore);

        var result = query.GetConversations("p-1", null, conversationStore, messageStore);

        Assert.All(result, c => Assert.Equal("p-1", c.ProjectId));
        Assert.DoesNotContain(result, c => c.ProjectId == "p-2");
    }

    [Fact]
    public void GetConversations_FiltersByTopic_CaseInsensitive()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var query = new ConversationQuery();

        var result = query.GetConversations(null, "kick", conversationStore, messageStore);

        Assert.Single(result);
        Assert.Equal("Kickoff", result[0].Topic);
    }

    [Fact]
    public void GetConversations_ReturnsEmpty_WhenNoProjectMatches()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var query = new ConversationQuery();

        var result = query.GetConversations("p-nonexistent", null, conversationStore, messageStore);

        Assert.Empty(result);
    }

    [Fact]
    public void GetConversations_ReturnsEmpty_WhenNoTopicMatches()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var query = new ConversationQuery();

        var result = query.GetConversations(null, "nonexistent-topic", conversationStore, messageStore);

        Assert.Empty(result);
    }

    [Fact]
    public void GetConversationById_ReturnsConversation_WhenExists()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var query = new ConversationQuery();

        var conversation = query.GetConversationById("c-1", conversationStore, messageStore);

        Assert.NotNull(conversation);
        Assert.Equal("c-1", conversation!.Id);
        Assert.Equal("p-1", conversation.ProjectId);
        Assert.Equal("Kickoff", conversation.Topic);
    }

    [Fact]
    public void GetConversationById_AttachesMessages_WhenExists()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var query = new ConversationQuery();

        var conversation = query.GetConversationById("c-1", conversationStore, messageStore);

        Assert.NotNull(conversation);
        Assert.Single(conversation!.Messages);
        Assert.Equal("m-1", conversation.Messages[0].Id);
    }

    [Fact]
    public void GetConversationById_ReturnsNull_WhenNotFound()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var query = new ConversationQuery();

        var conversation = query.GetConversationById("nonexistent-id", conversationStore, messageStore);

        Assert.Null(conversation);
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    [Fact]
    public void CreateConversation_ReturnsNewConversation_WithCorrectFields()
    {
        var conversationStore = new ConversationStore();
        var mutation = new ConversationMutation();

        var created = mutation.CreateConversation("p-42", "Design Review", conversationStore);

        Assert.NotNull(created);
        Assert.False(string.IsNullOrWhiteSpace(created.Id));
        Assert.Equal("p-42", created.ProjectId);
        Assert.Equal("Design Review", created.Topic);
    }

    [Fact]
    public void CreateConversation_IsRetrievableViaGetConversationById()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var mutation = new ConversationMutation();
        var query = new ConversationQuery();

        var created = mutation.CreateConversation("p-10", "Retro", conversationStore);
        var fetched = query.GetConversationById(created.Id, conversationStore, messageStore);

        Assert.NotNull(fetched);
        Assert.Equal("Retro", fetched!.Topic);
        Assert.Equal("p-10", fetched.ProjectId);
    }

    [Fact]
    public void CreateConversation_AppearsInGetConversations()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var mutation = new ConversationMutation();
        var query = new ConversationQuery();

        var created = mutation.CreateConversation("p-99", "Sprint Planning", conversationStore);
        var all = query.GetConversations(null, null, conversationStore, messageStore);

        Assert.Contains(all, c => c.Id == created.Id && c.Topic == "Sprint Planning");
    }

    [Fact]
    public void AddMessage_ReturnsNewMessage_WithCorrectFields()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var mutation = new ConversationMutation();

        var message = mutation.AddMessage("c-1", "Hello everyone!", "alice", conversationStore, messageStore);

        Assert.NotNull(message);
        Assert.False(string.IsNullOrWhiteSpace(message.Id));
        Assert.Equal("Hello everyone!", message.Text);
        Assert.Equal("alice", message.Author);
        Assert.Equal("c-1", message.Conversation!.Id);
    }

    [Fact]
    public void AddMessage_IsVisibleInGetConversationById()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var mutation = new ConversationMutation();
        var query = new ConversationQuery();

        mutation.AddMessage("c-1", "Follow-up", "bob", conversationStore, messageStore);
        var conversation = query.GetConversationById("c-1", conversationStore, messageStore);

        Assert.NotNull(conversation);
        Assert.Contains(conversation!.Messages, m => m.Text == "Follow-up" && m.Author == "bob");
    }

    [Fact]
    public void AddMessage_Throws_WhenConversationDoesNotExist()
    {
        var conversationStore = new ConversationStore();
        var messageStore = new MessageStore();
        var mutation = new ConversationMutation();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            mutation.AddMessage("nonexistent-conv", "Hi", "user", conversationStore, messageStore));

        Assert.Contains("nonexistent-conv", ex.Message);
    }
}
