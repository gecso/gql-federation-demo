using System.Text.Json;

namespace Conversation.Web.IntegrationTests;

public class MutationTests : GraphQLTestBase
{
    public MutationTests(GraphQLWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateConversation_ReturnsNewConversation_WithCorrectTitle()
    {
        var result = await ExecuteAsync(
            "mutation($title: String!) { createConversation(title: $title) { id title createdAt } }",
            new { title = "New Conversation" });

        Assert.False(HasErrors(result));
        var conv = Data(result).GetProperty("createConversation");
        Assert.False(string.IsNullOrEmpty(conv.GetProperty("id").GetString()));
        Assert.Equal("New Conversation", conv.GetProperty("title").GetString());
        Assert.NotEqual(JsonValueKind.Null, conv.GetProperty("createdAt").ValueKind);
    }

    [Fact]
    public async Task CreateConversation_AssignsUniqueId_ForEachConversation()
    {
        var r1 = await ExecuteAsync(
            "mutation { createConversation(title: \"Conv A\") { id } }");
        var r2 = await ExecuteAsync(
            "mutation { createConversation(title: \"Conv B\") { id } }");

        var id1 = Data(r1).GetProperty("createConversation").GetProperty("id").GetString();
        var id2 = Data(r2).GetProperty("createConversation").GetProperty("id").GetString();

        Assert.NotEqual(id1, id2);
    }

    [Fact]
    public async Task UpdateConversation_ReturnsUpdatedConversation_WhenItExists()
    {
        var created = await ExecuteAsync(
            "mutation { createConversation(title: \"Original\") { id } }");
        var id = Data(created).GetProperty("createConversation").GetProperty("id").GetString()!;

        var result = await ExecuteAsync(
            "mutation($id: String!, $title: String!) { updateConversation(id: $id, title: $title) { id title updatedAt } }",
            new { id, title = "Updated Title" });

        Assert.False(HasErrors(result));
        var conv = Data(result).GetProperty("updateConversation");
        Assert.Equal(id, conv.GetProperty("id").GetString());
        Assert.Equal("Updated Title", conv.GetProperty("title").GetString());
        Assert.NotEqual(JsonValueKind.Null, conv.GetProperty("updatedAt").ValueKind);
    }

    [Fact]
    public async Task UpdateConversation_ReturnsNull_WhenConversationDoesNotExist()
    {
        var result = await ExecuteAsync(
            "mutation($id: String!, $title: String!) { updateConversation(id: $id, title: $title) { id } }",
            new { id = "nonexistent", title = "New Title" });

        Assert.False(HasErrors(result));
        Assert.Equal(JsonValueKind.Null, Data(result).GetProperty("updateConversation").ValueKind);
    }

    [Fact]
    public async Task DeleteConversation_ReturnsTrue_WhenConversationExists()
    {
        var created = await ExecuteAsync(
            "mutation { createConversation(title: \"To Delete\") { id } }");
        var id = Data(created).GetProperty("createConversation").GetProperty("id").GetString()!;

        var result = await ExecuteAsync(
            "mutation($id: String!) { deleteConversation(id: $id) }",
            new { id });

        Assert.False(HasErrors(result));
        Assert.True(Data(result).GetProperty("deleteConversation").GetBoolean());
    }

    [Fact]
    public async Task DeleteConversation_ReturnsFalse_WhenConversationDoesNotExist()
    {
        var result = await ExecuteAsync(
            "mutation($id: String!) { deleteConversation(id: $id) }",
            new { id = "nonexistent-id" });

        Assert.False(HasErrors(result));
        Assert.False(Data(result).GetProperty("deleteConversation").GetBoolean());
    }

    [Fact]
    public async Task DeleteConversation_RemovesConversation_FromGetConversations()
    {
        var created = await ExecuteAsync(
            "mutation { createConversation(title: \"To Be Gone\") { id } }");
        var id = Data(created).GetProperty("createConversation").GetProperty("id").GetString()!;

        await ExecuteAsync("mutation($id: String!) { deleteConversation(id: $id) }", new { id });

        var result = await ExecuteAsync(
            "query($id: String!) { conversation(id: $id) { id } }",
            new { id });

        Assert.Equal(JsonValueKind.Null, Data(result).GetProperty("conversation").ValueKind);
    }

    [Fact]
    public async Task SendMessage_ReturnsNewMessage_WithCorrectFields()
    {
        var created = await ExecuteAsync(
            "mutation { createConversation(title: \"Chat\") { id } }");
        var convId = Data(created).GetProperty("createConversation").GetProperty("id").GetString()!;

        var result = await ExecuteAsync(
            "mutation($conversationId: String!, $content: String!, $authorId: String!) { sendMessage(conversationId: $conversationId, content: $content, authorId: $authorId) { id conversationId content authorId sentAt } }",
            new { conversationId = convId, content = "Hi there!", authorId = "user-42" });

        Assert.False(HasErrors(result));
        var msg = Data(result).GetProperty("sendMessage");
        Assert.False(string.IsNullOrEmpty(msg.GetProperty("id").GetString()));
        Assert.Equal(convId, msg.GetProperty("conversationId").GetString());
        Assert.Equal("Hi there!", msg.GetProperty("content").GetString());
        Assert.Equal("user-42", msg.GetProperty("authorId").GetString());
        Assert.NotEqual(JsonValueKind.Null, msg.GetProperty("sentAt").ValueKind);
    }

    [Fact]
    public async Task SendMessage_ReturnsNull_WhenConversationDoesNotExist()
    {
        var result = await ExecuteAsync(
            "mutation($conversationId: String!, $content: String!, $authorId: String!) { sendMessage(conversationId: $conversationId, content: $content, authorId: $authorId) { id } }",
            new { conversationId = "nonexistent", content = "Hello", authorId = "user-1" });

        Assert.False(HasErrors(result));
        Assert.Equal(JsonValueKind.Null, Data(result).GetProperty("sendMessage").ValueKind);
    }

    [Fact]
    public async Task DeleteMessage_ReturnsTrue_WhenMessageExists()
    {
        var created = await ExecuteAsync(
            "mutation { createConversation(title: \"Del Msg Conv\") { id } }");
        var convId = Data(created).GetProperty("createConversation").GetProperty("id").GetString()!;

        var sent = await ExecuteAsync(
            "mutation($conversationId: String!, $content: String!, $authorId: String!) { sendMessage(conversationId: $conversationId, content: $content, authorId: $authorId) { id } }",
            new { conversationId = convId, content = "Delete me", authorId = "u1" });
        var msgId = Data(sent).GetProperty("sendMessage").GetProperty("id").GetString()!;

        var result = await ExecuteAsync(
            "mutation($id: String!) { deleteMessage(id: $id) }",
            new { id = msgId });

        Assert.False(HasErrors(result));
        Assert.True(Data(result).GetProperty("deleteMessage").GetBoolean());
    }

    [Fact]
    public async Task DeleteMessage_ReturnsFalse_WhenMessageDoesNotExist()
    {
        var result = await ExecuteAsync(
            "mutation($id: String!) { deleteMessage(id: $id) }",
            new { id = "nonexistent-msg" });

        Assert.False(HasErrors(result));
        Assert.False(Data(result).GetProperty("deleteMessage").GetBoolean());
    }

    [Fact]
    public async Task DeleteMessage_RemovesMessage_FromGetMessages()
    {
        var created = await ExecuteAsync(
            "mutation { createConversation(title: \"Msg Removal\") { id } }");
        var convId = Data(created).GetProperty("createConversation").GetProperty("id").GetString()!;

        var sent = await ExecuteAsync(
            "mutation($conversationId: String!, $content: String!, $authorId: String!) { sendMessage(conversationId: $conversationId, content: $content, authorId: $authorId) { id } }",
            new { conversationId = convId, content = "Bye", authorId = "u2" });
        var msgId = Data(sent).GetProperty("sendMessage").GetProperty("id").GetString()!;

        await ExecuteAsync("mutation($id: String!) { deleteMessage(id: $id) }", new { id = msgId });

        var result = await ExecuteAsync(
            "query($id: String!) { message(id: $id) { id } }",
            new { id = msgId });

        Assert.Equal(JsonValueKind.Null, Data(result).GetProperty("message").ValueKind);
    }
}
