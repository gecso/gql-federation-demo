using HotChocolate.Fusion;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services
	.AddFusionGatewayServer()
	.ConfigureFromFile("fusion-gateway.graphql", watchFileForUpdates: true);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
app.MapGraphQL("/graphql");

app.Run();
