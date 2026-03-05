using Conversation.Web.GraphQL;
using Conversation.Web.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ConversationStore>();
builder.Services.AddSingleton<MessageStore>();

builder.Services
	.AddGraphQLServer()
	.AddQueryType<ConversationQuery>()
	.AddMutationType<ConversationMutation>()
	.AddTypeExtension<ConversationResolvers>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGraphQL("/graphql");

app.Run();

public partial class Program;
