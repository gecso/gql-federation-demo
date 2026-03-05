using System.Text.Json;

namespace Conversation.Web.IntegrationTests;

/// <summary>
/// Tests that need a clean (empty) state use their own factory instance.
/// </summary>
public class QueryTests_EmptyState : GraphQLTestBase
{
    public QueryTests_EmptyState(GraphQLWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetConversations_ReturnsEmptyList_WhenNoConversationsExist()
    {
        var result = await ExecuteAsync("{ conversations { id title createdAt } }");

        Assert.False(HasErrors(result));
        var conversations = Data(result).GetProperty("conversations");
        Assert.Equal(JsonValueKind.Array, conversations.ValueKind);
        Assert.Equal(0, conversations.GetArrayLength());
    }
}

public class QueryTests : GraphQLTestBase
{
    public QueryTests(GraphQLWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetConversations_ReturnsAllConversations_AfterCreation()
    {
        await ExecuteAsync(
            "mutation($title: String!) { createConversation(title: $title) { id } }",
            new { title = "Query Test Conv" });

        var result = await ExecuteAsync("{ conversations { id title createdAt } }");

        Assert.False(HasErrors(result));
        var conversations = Data(result).GetProperty("conversations");
        Assert.True(conversations.GetArrayLength() >= 1);
    }

    [Fact]
    public async Task GetConversation_ReturnsConversation_WhenItExists()
    {
        var created = await ExecuteAsync(
            "mutation($title: String!) { createConversation(title: $title) { id title } }",
            new { title = "Specific Conv" });
        var id = Data(created).GetProperty("createConversation").GetProperty("id").GetString()!;

        var result = await ExecuteAsync(
            "query($id: String!) { conversation(id: $id) { id title } }",
            new { id });

        Assert.False(HasErrors(result));
        var conv = Data(result).GetProperty("conversation");
        Assert.Equal(id, conv.GetProperty("id").GetString());
        Assert.Equal("Specific Conv", conv.GetProperty("title").GetString());
    }

    [Fact]
    public async Task GetConversation_ReturnsNull_WhenNotFound()
    {
        var result = await ExecuteAsync(
            "query($id: String!) { conversation(id: $id) { id } }",
            new { id = "nonexistent-id" });

        Assert.False(HasErrors(result));
        Assert.Equal(JsonValueKind.Null, Data(result).GetProperty("conversation").ValueKind);
    }

    [Fact]
    public async Task GetMessages_ReturnsMessages_ForConversation()
    {
        var created = await ExecuteAsync(
            "mutation($title: String!) { createConversation(title: $title) { id } }",
            new { title = "Msg Conv" });
        var convId = Data(created).GetProperty("createConversation").GetProperty("id").GetString()!;

        await ExecuteAsync(
            "mutation($conversationId: String!, $content: String!, $authorId: String!) { sendMessage(conversationId: $conversationId, content: $content, authorId: $authorId) { id } }",
            new { conversationId = convId, content = "Hello!", authorId = "user-1" });

        var result = await ExecuteAsync(
            "query($conversationId: String!) { messages(conversationId: $conversationId) { id content authorId } }",
            new { conversationId = convId });

        Assert.False(HasErrors(result));
        var messages = Data(result).GetProperty("messages");
        Assert.Equal(1, messages.GetArrayLength());
        Assert.Equal("Hello!", messages[0].GetProperty("content").GetString());
        Assert.Equal("user-1", messages[0].GetProperty("authorId").GetString());
    }

    [Fact]
    public async Task GetMessages_ReturnsEmptyList_WhenNoMessages()
    {
        var created = await ExecuteAsync(
            "mutation($title: String!) { createConversation(title: $title) { id } }",
            new { title = "Empty Msg Conv" });
        var convId = Data(created).GetProperty("createConversation").GetProperty("id").GetString()!;

        var result = await ExecuteAsync(
            "query($conversationId: String!) { messages(conversationId: $conversationId) { id } }",
            new { conversationId = convId });

        Assert.False(HasErrors(result));
        var messages = Data(result).GetProperty("messages");
        Assert.Equal(0, messages.GetArrayLength());
    }

    [Fact]
    public async Task GetMessage_ReturnsMessage_WhenItExists()
    {
        var created = await ExecuteAsync(
            "mutation($title: String!) { createConversation(title: $title) { id } }",
            new { title = "Single Msg Conv" });
        var convId = Data(created).GetProperty("createConversation").GetProperty("id").GetString()!;

        var sentResult = await ExecuteAsync(
            "mutation($conversationId: String!, $content: String!, $authorId: String!) { sendMessage(conversationId: $conversationId, content: $content, authorId: $authorId) { id content } }",
            new { conversationId = convId, content = "Test message", authorId = "author-1" });
        var msgId = Data(sentResult).GetProperty("sendMessage").GetProperty("id").GetString()!;

        var result = await ExecuteAsync(
            "query($id: String!) { message(id: $id) { id content authorId sentAt } }",
            new { id = msgId });

        Assert.False(HasErrors(result));
        var msg = Data(result).GetProperty("message");
        Assert.Equal(msgId, msg.GetProperty("id").GetString());
        Assert.Equal("Test message", msg.GetProperty("content").GetString());
    }

    [Fact]
    public async Task GetMessage_ReturnsNull_WhenNotFound()
    {
        var result = await ExecuteAsync(
            "query($id: String!) { message(id: $id) { id } }",
            new { id = "nonexistent-msg-id" });

        Assert.False(HasErrors(result));
        Assert.Equal(JsonValueKind.Null, Data(result).GetProperty("message").ValueKind);
    }
}
