using ProjectModel = Project.Web.Models.Project;

namespace Project.Web.Storage;

public sealed class ProjectStore
{
    private readonly object _sync = new();

    private readonly List<ProjectModel> _projects =
    [
        new()
        {
            Id = "p-1",
            Name = "Demo project",
            Description = "Проект-заготовка для Federation demo"
        },
        new()
        {
            Id = "p-2",
            Name = "Internal Tools",
            Description = "Тестовый проект для поиска"
        }
    ];

    public IReadOnlyList<ProjectModel> GetAll()
    {
        lock (_sync)
        {
            return _projects.Select(Clone).ToArray();
        }
    }

    public ProjectModel? GetById(string id)
    {
        lock (_sync)
        {
            var project = _projects.FirstOrDefault(x => x.Id == id);
            return project is null ? null : Clone(project);
        }
    }

    public IReadOnlyList<ProjectModel> SearchByName(string name)
    {
        var term = (name ?? string.Empty).Trim();
        if (term.Length == 0)
        {
            return [];
        }

        lock (_sync)
        {
            return _projects
                .Where(x => x.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(Clone)
                .ToArray();
        }
    }

    public ProjectModel Create(string name, string description)
    {
        var project = new ProjectModel
        {
            Name = name,
            Description = description
        };

        lock (_sync)
        {
            _projects.Add(project);
            return Clone(project);
        }
    }

    private static ProjectModel Clone(ProjectModel source) =>
        new()
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description
        };
}
