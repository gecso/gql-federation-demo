using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;

namespace Conversation.Web.IntegrationTests;

public class GraphQLWebApplicationFactory : WebApplicationFactory<Program>
{
}

public abstract class GraphQLTestBase : IClassFixture<GraphQLWebApplicationFactory>
{
    private readonly HttpClient _client;

    protected GraphQLTestBase(GraphQLWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    protected async Task<JsonElement> ExecuteAsync(string query, object? variables = null)
    {
        var request = new { query, variables };
        var response = await _client.PostAsJsonAsync("/graphql", request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json;
    }

    protected static JsonElement Data(JsonElement result)
        => result.GetProperty("data");

    protected static bool HasErrors(JsonElement result)
        => result.TryGetProperty("errors", out var errors) && errors.GetArrayLength() > 0;
}
