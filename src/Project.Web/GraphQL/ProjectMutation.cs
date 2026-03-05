using ProjectModel = Project.Web.Models.Project;
using Project.Web.Storage;
using HotChocolate;

namespace Project.Web.GraphQL;

[GraphQLName("Mutation")]
public sealed class ProjectMutation
{
    public ProjectModel CreateProject(string name, string description, [Service] ProjectStore store) =>
        store.Create(name, description);
}
