using ProjectModel = Project.Web.Models.Project;
using Project.Web.Storage;

namespace Project.Web.GraphQL;

public sealed class ProjectMutation
{
    public ProjectModel CreateProject(string name, string description, [Service] ProjectStore store) =>
        store.Create(name, description);
}
