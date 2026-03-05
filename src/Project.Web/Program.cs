using Project.Web.GraphQL;
using Project.Web.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ProjectStore>();

builder.Services
	.AddGraphQLServer()
	.AddQueryType<ProjectQuery>()
	.AddMutationType<ProjectMutation>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGraphQL("/graphql");

app.Run();

public partial class Program;
