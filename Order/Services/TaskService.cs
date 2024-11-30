using Microsoft.EntityFrameworkCore;
using Order;

public class TaskService
{
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Метод для отвязывания задач от проекта
    public async Task UnassignTasksFromProject(int projectId)
    {
        var tasks = await _context.Tasks.Where(t => t.ProjectId == projectId).ToListAsync();
        foreach (var task in tasks)
        {
            task.ProjectId = null;
            _context.Entry(task).State = EntityState.Modified;
        }
        await _context.SaveChangesAsync();
    }

    // Метод для привязки задач к проекту
    public async Task AssignTasksToProject(int projectId, List<int> taskIds)
    {
        var tasks = await _context.Tasks.Where(t => taskIds.Contains(t.Id)).ToListAsync();
        foreach (var task in tasks)
        {
            task.ProjectId = projectId;
            _context.Entry(task).State = EntityState.Modified;
        }
        await _context.SaveChangesAsync();
    }

    // Метод для отвязывания задач от события
    public async Task UnassignTasksFromEvent(int eventId)
    {
        var tasks = await _context.Tasks.Where(t => t.EventId == eventId).ToListAsync();
        foreach (var task in tasks)
        {
            task.EventId = null;
            _context.Entry(task).State = EntityState.Modified;
        }
        await _context.SaveChangesAsync();
    }

    // Метод для привязки задач к событию
    public async Task AssignTasksToEvent(int eventId, List<int> taskIds)
    {
        var tasks = await _context.Tasks.Where(t => taskIds.Contains(t.Id)).ToListAsync();
        foreach (var task in tasks)
        {
            task.EventId = eventId;
            _context.Entry(task).State = EntityState.Modified;
        }
        await _context.SaveChangesAsync();
    }

    // Метод для удаления задачи из события и проекта, к которому она привязана
    public async Task RemoveTask(int taskId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
            throw new ArgumentException("Task not found");

        var project = await _context.Projects.FindAsync(task.ProjectId);

        if (project != null)
        {
            project.TaskIds.Remove(taskId); // Удаляем ID задачи из списка TaskIds
            _context.Entry(project).State = EntityState.Modified;
        }

        var evt = await _context.Events.FindAsync(task.EventId);

        if (evt != null)
        {
            evt.TaskIds.Remove(taskId); // Удаляем ID задачи из списка TaskIds
            _context.Entry(evt).State = EntityState.Modified;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }
}
