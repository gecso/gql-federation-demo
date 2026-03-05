using Project.Web.GraphQL;
using Project.Web.Storage;

namespace Project.Web.Tests;

public class ProjectGraphQlTests
{
    [Fact]
    public void GetProjects_ReturnsSeedProjects()
    {
        var store = new ProjectStore();
        var query = new ProjectQuery();

        var projects = query.GetProjects(store);

        Assert.Equal(2, projects.Count);
        Assert.Contains(projects, p => p.Id == "p-1");
        Assert.Contains(projects, p => p.Id == "p-2");
    }

    [Fact]
    public void GetProjectById_ReturnsProject_WhenExists()
    {
        var store = new ProjectStore();
        var query = new ProjectQuery();

        var project = query.GetProjectById("p-1", store);

        Assert.NotNull(project);
        Assert.Equal("p-1", project!.Id);
        Assert.Equal("Demo project", project.Name);
    }

    [Fact]
    public void GetProjectById_ReturnsNull_WhenNotFound()
    {
        var store = new ProjectStore();
        var query = new ProjectQuery();

        var project = query.GetProjectById("missing-id", store);

        Assert.Null(project);
    }

    [Fact]
    public void SearchProjectsByName_FindsProjects_CaseInsensitive()
    {
        var store = new ProjectStore();
        var query = new ProjectQuery();

        var projects = query.SearchProjectsByName("demo", store);

        Assert.Single(projects);
        Assert.Equal("p-1", projects[0].Id);
    }

    [Fact]
    public void CreateProject_AddsProject_AndCanBeFetchedById()
    {
        var store = new ProjectStore();
        var mutation = new ProjectMutation();
        var query = new ProjectQuery();

        var created = mutation.CreateProject("GraphQL Gateway", "BFF project", store);
        var fetched = query.GetProjectById(created.Id, store);

        Assert.NotNull(fetched);
        Assert.Equal("GraphQL Gateway", fetched!.Name);
        Assert.Equal("BFF project", fetched.Description);
    }
}
