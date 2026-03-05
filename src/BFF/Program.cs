using BFF.GraphQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddGraphQLServer()
	.AddQueryType<BffQuery>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGraphQL("/graphql");

app.Run();
