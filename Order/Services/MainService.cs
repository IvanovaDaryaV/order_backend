using Microsoft.EntityFrameworkCore;
using Order;
using Order.Models;
using System.Reflection;
using System.Text.Json;

public class MainService
{
    private readonly ApplicationDbContext _context;

    public MainService(ApplicationDbContext context)
    {
        _context = context;
    }

    // Метод для отвязывания задач от проекта
    public async System.Threading.Tasks.Task UnassignTasksFromProject(int projectId)
    {
        var tasks = await _context.Tasks.Where(t => t.ProjectId == projectId).ToListAsync();
        foreach (var task in tasks)
        {
            Console.WriteLine(task.TaskId);
            task.ProjectId = null;
            _context.Entry(task).State = EntityState.Modified;
        }
        await _context.SaveChangesAsync();
        Console.WriteLine("ЗАДАЧИ УСПЕШНО ОТВЯЗАНЫ ОТ ПРОЕКТА");
    }

    // Метод для привязки задач к проекту
    public async System.Threading.Tasks.Task AssignTasksToProject(int projectId, List<int> taskIds)
    {
        var tasks = await _context.Tasks.Where(t => taskIds.Contains(t.TaskId)).ToListAsync();
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId);

        if (project == null)
        {
            throw new ArgumentException("Project not found");
        }


        foreach (var task in tasks)
        {
            task.ProjectId = projectId;
            Console.WriteLine(task.TaskId);
            // Логика наследования контекста:
            // если проект имеет контекст, задачи его наследует
            // если нет, у задач будет свой собственный, либо null

            // Даже если задача уже имеет свой контекст,
            // он будет перезаписан

            if (project.ContextId != null)
            {
                task.ContextId = project.ContextId;
            }

            //_context.Entry(task).State = EntityState.Modified;
            _context.Tasks.Update(task);
        }
        var result = await _context.SaveChangesAsync();
        Console.WriteLine($"Modified: {result}");
        Console.WriteLine("ЗАДАЧИ УСПЕШНО ПРИВЯЗАНЫ К ПРОЕКТУ");
    }
    // Метод для отвязывания заметок от проекта
    public async System.Threading.Tasks.Task UnassignNotesFromProject(int? projectId)
    {
        var notes = await _context.Notes.Where(n => n.ProjectId == projectId).ToListAsync();
        foreach (var note in notes)
        {
            note.ProjectId = null;
            _context.Entry(note).State = EntityState.Modified;
        }
        await _context.SaveChangesAsync();
    }

    // Метод для привязки заметок к проекту
    public async System.Threading.Tasks.Task AssignNotesToProject(int? projectId, List<int> noteIds)
    {
        var notes = await _context.Notes.Where(t => noteIds.Contains(t.NoteId)).ToListAsync(); // получаем заметки, которые есть в переданном списке
        var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId);

        if (project == null)
        {
            throw new ArgumentException("Project not found");
        }


        foreach (var note in notes)
        {
            note.ProjectId = projectId;
            _context.Entry(note).State = EntityState.Modified;
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
        var tasks = await _context.Tasks.Where(t => taskIds.Contains(t.TaskId)).ToListAsync();

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

        if (project != null && project.TaskIds != null)
        {
            project.TaskIds.Remove(taskId); // Удаляем ID задачи из списка TaskIds
            _context.Entry(project).State = EntityState.Modified;
        }

        var evt = await _context.Events.FindAsync(task.EventId);

        if (evt != null && evt.TaskIds != null)
        {
            evt.TaskIds.Remove(taskId); // Удаляем ID задачи из списка TaskIds
            _context.Entry(evt).State = EntityState.Modified;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
    }

    // Метод для принудительной установки null значений
    // универсальный для любых классов
    public void SetNullFields<T>(T obj, Dictionary<string, object> jsonDict) where T : class
    {
        foreach (var key in jsonDict.Keys)
        {
            if (jsonDict[key] == null)
            {
                var propertyName = char.ToUpper(key[0]) + key.Substring(1);
                var property = obj.GetType().GetProperty(propertyName);

                if (property != null && property.CanWrite)
                {
                    property.SetValue(obj, null);
                }
            }
        }
    }

    // Валидация для полей, которые не должны зануляться
    public void ValidateEntityForNotNullConstraints(object entity)
    {
        var type = entity.GetType();
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Проверяем, допускает ли свойство значение null
            var propertyType = property.PropertyType;
            bool isNullable = !propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) != null;

            // Получаем значение свойства
            var value = property.GetValue(entity);

            // Если свойство не nullable и значение null, генерируем исключение
            if (!isNullable && value == null)
            {
                throw new InvalidOperationException($"Поле '{property.Name}' не может быть null.");
            }
        }
    }


}
