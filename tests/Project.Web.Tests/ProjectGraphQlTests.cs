using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Project.Web.Tests;

public class ProjectGraphQlTests
{
    [Fact]
    public async Task Root_Get_ReturnsHelloWorld()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal("Hello World!", body);
    }

    [Fact]
    public async Task GetProjects_ReturnsSeedProjects()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        const string gql = """
                        query {
                            projects {
                                id
                                name
                                description
                            }
                        }
                        """;

        var root = await PostGraphQlAsync(client, gql);
        var projects = root.GetProperty("data").GetProperty("projects");

        Assert.Equal(2, projects.GetArrayLength());
        Assert.Contains(projects.EnumerateArray(), p => p.GetProperty("id").GetString() == "p-1");
        Assert.Contains(projects.EnumerateArray(), p => p.GetProperty("id").GetString() == "p-2");
    }

    [Fact]
    public async Task GetProjectById_ReturnsProject_WhenExists()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        const string gql = """
                        query($id: String!) {
                            projectById(id: $id) {
                                id
                                name
                            }
                        }
                        """;

        var root = await PostGraphQlAsync(client, gql, new { id = "p-1" });
        var project = root.GetProperty("data").GetProperty("projectById");

        Assert.Equal("p-1", project.GetProperty("id").GetString());
        Assert.Equal("Demo project", project.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetProjectById_ReturnsNull_WhenNotFound()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        const string gql = """
                        query($id: String!) {
                            projectById(id: $id) {
                                id
                            }
                        }
                        """;

        var root = await PostGraphQlAsync(client, gql, new { id = "missing-id" });
        var project = root.GetProperty("data").GetProperty("projectById");

        Assert.Equal(JsonValueKind.Null, project.ValueKind);
    }

    [Fact]
    public async Task SearchProjectsByName_FindsProjects_CaseInsensitive()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        const string gql = """
                        query($name: String!) {
                            searchProjectsByName(name: $name) {
                                id
                            }
                        }
                        """;

        var root = await PostGraphQlAsync(client, gql, new { name = "demo" });
        var projects = root.GetProperty("data").GetProperty("searchProjectsByName");

        Assert.Equal(1, projects.GetArrayLength());
        Assert.Equal("p-1", projects[0].GetProperty("id").GetString());
    }

    [Fact]
    public async Task CreateProject_AddsProject_AndCanBeFetchedById()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        const string createGql = """
                        mutation($name: String!, $description: String!) {
                            createProject(name: $name, description: $description) {
                                id
                                name
                                description
                            }
                        }
                        """;

        var createRoot = await PostGraphQlAsync(client, createGql, new { name = "GraphQL Gateway", description = "BFF project" });
        var created = createRoot.GetProperty("data").GetProperty("createProject");
        var createdId = created.GetProperty("id").GetString();

        Assert.False(string.IsNullOrWhiteSpace(createdId));
        Assert.Equal("GraphQL Gateway", created.GetProperty("name").GetString());

        const string fetchGql = """
                        query($id: String!) {
                            projectById(id: $id) {
                                id
                                name
                                description
                            }
                        }
                        """;

        var fetchRoot = await PostGraphQlAsync(client, fetchGql, new { id = createdId });
        var fetched = fetchRoot.GetProperty("data").GetProperty("projectById");

        Assert.Equal(createdId, fetched.GetProperty("id").GetString());
        Assert.Equal("GraphQL Gateway", fetched.GetProperty("name").GetString());
        Assert.Equal("BFF project", fetched.GetProperty("description").GetString());
    }

    private static async Task<JsonElement> PostGraphQlAsync(HttpClient client, string query, object? variables = null)
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

        if (root.TryGetProperty("errors", out var errors))
        {
            Assert.Fail($"GraphQL returned errors: {errors}");
        }

        return root;
    }
}
