using Microsoft.EntityFrameworkCore;
using Order;
using Order.Models;

public class TaskService
{
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Метод для отвязывания задач от проекта
    public async System.Threading.Tasks.Task UnassignTasksFromProject(int projectId)
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
    public async System.Threading.Tasks.Task AssignTasksToProject(int projectId, List<int> taskIds)
    {
        var tasks = await _context.Tasks.Where(t => taskIds.Contains(t.Id)).ToListAsync();
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

        foreach (var task in tasks)
        {
            task.ProjectId = projectId;

            // Логика наследования контекста:
            // если проект имеет контекст, задачи его наследует
            // если нет, у задач будет свой собственный, либо null

            // Даже если задача уже имеет свой контекст,
            // он будет перезаписан

            if (project.ContextId != null)
            {
                task.ContextId = project.ContextId;
            }

            _context.Entry(task).State = EntityState.Modified;
        }
        await _context.SaveChangesAsync();
    }

    // Метод для отвязывания задач от события
    public async System.Threading.Tasks.Task UnassignTasksFromEvent(int eventId)
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
    public async System.Threading.Tasks.Task AssignTasksToEvent(int eventId, List<int> taskIds)
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
    public async System.Threading.Tasks.Task RemoveTask(int taskId)
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
