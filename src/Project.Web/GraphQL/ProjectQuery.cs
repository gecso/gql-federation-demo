using ProjectModel = Project.Web.Models.Project;
using Project.Web.Storage;

namespace Project.Web.GraphQL;

public sealed class ProjectQuery
{
    public IReadOnlyList<ProjectModel> GetProjects([Service] ProjectStore store) =>
        store.GetAll();

    public ProjectModel? GetProjectById(string id, [Service] ProjectStore store) =>
        store.GetById(id);

    public IReadOnlyList<ProjectModel> SearchProjectsByName(string name, [Service] ProjectStore store) =>
        store.SearchByName(name);
}
