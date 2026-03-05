using Conversation.Web.GraphQL;
using Conversation.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConversationService, InMemoryConversationService>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>();

var app = builder.Build();

app.MapGraphQL();

app.Run();

public partial class Program { }
