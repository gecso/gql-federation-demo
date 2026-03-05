using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Conversation.Web.Tests;

public class ConversationGraphQlIntegrationTests
{
        [Fact]
        public async Task GetConversations_ReturnsSeedConversation_WithMessages()
    {
                await using var factory = new WebApplicationFactory<Program>();
                using var client = factory.CreateClient();

                const string gql = """
                        query {
                            conversations {
                                id
                                projectId
                                topic
                                messages {
                                    id
                                    text
                                    author
                                }
                            }
                        }
                        """;

                var root = await PostGraphQlAsync(client, gql);
                var conversations = root.GetProperty("data").GetProperty("conversations");

                Assert.True(conversations.GetArrayLength() >= 1);
                var seed = conversations.EnumerateArray().First(c => c.GetProperty("id").GetString() == "c-1");
                var messages = seed.GetProperty("messages");
                Assert.Equal(1, messages.GetArrayLength());
                Assert.Equal("m-1", messages[0].GetProperty("id").GetString());
    }

    [Fact]
        public async Task GetConversations_FiltersByProjectId_AndTopic()
    {
                await using var factory = new WebApplicationFactory<Program>();
                using var client = factory.CreateClient();

                const string createGql = """
                        mutation($projectId: String!, $topic: String!) {
                            createConversation(projectId: $projectId, topic: $topic) {
                                id
                            }
                        }
                        """;

                await PostGraphQlAsync(client, createGql, new { projectId = "p-2", topic = "Roadmap" });

                const string listGql = """
                        query($projectId: String, $topic: String) {
                            conversations(projectId: $projectId, topic: $topic) {
                                id
                                projectId
                                topic
                            }
                        }
                        """;

                var root = await PostGraphQlAsync(client, listGql, new { projectId = "p-2", topic = "road" });
                var conversations = root.GetProperty("data").GetProperty("conversations");

                Assert.Equal(1, conversations.GetArrayLength());
                Assert.Equal("p-2", conversations[0].GetProperty("projectId").GetString());
                Assert.Equal("Roadmap", conversations[0].GetProperty("topic").GetString());
        }

        [Fact]
        public async Task GetConversationById_ReturnsConversation_WhenExists()
        {
                await using var factory = new WebApplicationFactory<Program>();
                using var client = factory.CreateClient();

                const string gql = """
                        query($id: String!) {
                            conversationById(id: $id) {
                                id
                                projectId
                                topic
                            }
                        }
                        """;

                var root = await PostGraphQlAsync(client, gql, new { id = "c-1" });
                var conversation = root.GetProperty("data").GetProperty("conversationById");

                Assert.Equal("c-1", conversation.GetProperty("id").GetString());
                Assert.Equal("p-1", conversation.GetProperty("projectId").GetString());
        }

        [Fact]
        public async Task CreateConversation_CreatesEntity_ViaGraphQl()
        {
                await using var factory = new WebApplicationFactory<Program>();
                using var client = factory.CreateClient();

                const string gql = """
                        mutation($projectId: String!, $topic: String!) {
                            createConversation(projectId: $projectId, topic: $topic) {
                                id
                                projectId
                                topic
                            }
                        }
                        """;

                var root = await PostGraphQlAsync(client, gql, new { projectId = "p-9", topic = "Architecture" });
                var created = root.GetProperty("data").GetProperty("createConversation");

                Assert.False(string.IsNullOrWhiteSpace(created.GetProperty("id").GetString()));
                Assert.Equal("p-9", created.GetProperty("projectId").GetString());
                Assert.Equal("Architecture", created.GetProperty("topic").GetString());
    }

    [Fact]
        public async Task AddMessage_AddsMessageToConversation_ViaGraphQl()
    {
                await using var factory = new WebApplicationFactory<Program>();
                using var client = factory.CreateClient();

                const string createConversationGql = """
                        mutation($projectId: String!, $topic: String!) {
                            createConversation(projectId: $projectId, topic: $topic) {
                                id
                            }
                        }
                        """;

                var createRoot = await PostGraphQlAsync(client, createConversationGql, new { projectId = "p-1", topic = "Daily" });
                var conversationId = createRoot.GetProperty("data").GetProperty("createConversation").GetProperty("id").GetString();

                const string addMessageGql = """
                        mutation($conversationId: String!, $text: String!, $author: String!) {
                            addMessage(conversationId: $conversationId, text: $text, author: $author) {
                                id
                                text
                                author
                                conversation {
                                    id
                                }
                            }
                        }
                        """;

                var addRoot = await PostGraphQlAsync(
                        client,
                        addMessageGql,
                        new { conversationId, text = "Hello", author = "alice" });

                var message = addRoot.GetProperty("data").GetProperty("addMessage");
                Assert.Equal("Hello", message.GetProperty("text").GetString());
                Assert.Equal("alice", message.GetProperty("author").GetString());
                Assert.Equal(conversationId, message.GetProperty("conversation").GetProperty("id").GetString());

                const string queryConversationGql = """
                        query($id: String!) {
                            conversationById(id: $id) {
                                id
                                messages {
                                    text
                                    author
                                }
                            }
                        }
                        """;

                var queryRoot = await PostGraphQlAsync(client, queryConversationGql, new { id = conversationId });
                var messages = queryRoot.GetProperty("data").GetProperty("conversationById").GetProperty("messages");

                Assert.Equal(1, messages.GetArrayLength());
                Assert.Equal("Hello", messages[0].GetProperty("text").GetString());
        }

        [Fact]
        public async Task AddMessage_ReturnsGraphQlError_WhenConversationMissing()
        {
                await using var factory = new WebApplicationFactory<Program>();
                using var client = factory.CreateClient();

                const string gql = """
                        mutation {
                            addMessage(conversationId: "missing-id", text: "Hello", author: "alice") {
                                id
                            }
                        }
                        """;

                var root = await PostGraphQlAsync(client, gql, expectErrors: true);
                var errors = root.GetProperty("errors");

                Assert.True(errors.GetArrayLength() >= 1);
                var firstError = errors[0];
                Assert.Equal("Unexpected Execution Error", firstError.GetProperty("message").GetString());
                Assert.Equal("addMessage", firstError.GetProperty("path")[0].GetString());
        }

        private static async Task<JsonElement> PostGraphQlAsync(
                HttpClient client,
                string query,
                object? variables = null,
                bool expectErrors = false)
        {
                var payload = new
                {
                        query,
                        variables
                };

                using var response = await client.PostAsJsonAsync("/graphql", payload);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(content);
                var root = document.RootElement.Clone();

                if (!expectErrors && root.TryGetProperty("errors", out var errors))
                {
                        Assert.Fail($"GraphQL returned errors: {errors}");
                }

                return root;
    }
}
