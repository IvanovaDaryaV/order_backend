using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.Models;
using Order.Models.DTO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Order.Controllers.EntitiesControllers
{
    [ApiController]
    //[Authorize]
    [Route("api/[controller]")]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TaskController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Task/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();
            return Ok(task);
        }

        // Получение задач для фильтрации по контексту
        // GET: api/Task/{id}
        [HttpGet("/api/Task/user/")]
        public async Task<IActionResult> GetAllTasks(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Tasks)
                .Include(u => u.Projects)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound();

            var tasks = user.Tasks;
            var projects = user.Projects;

            if (tasks == null)
                return NotFound();

            var contextIds = tasks
                .Where(task => task.ContextId != null)
                .Select(task => task.ContextId)
                .Distinct()
                .ToList();

            var contexts = await _context.Contexts
                .Where(c => contextIds.Contains(c.Id))
                .ToListAsync();

            var result = new Dictionary<string, List<object>>();

            foreach (var context in contexts)
            {
                // Для каждого контекста ищем задачи и проекты, связанные с этим контекстом
                var contextTasks = tasks
                    .Where(task => task.ContextId == context.Id)
                    .Select(task => new { task.Name, task.Project })
                    .ToList();

                if (contextTasks.Any())
                {
                    result[context.Name] = contextTasks.Cast<object>().ToList();
                }
            }

            return Ok(result);
        }

        // POST: api/Task
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] Models.Task newTask, [FromServices] MainService taskService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, newTask);
        }

        // PUT: api/Task/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] JsonElement body, [FromServices] MainService mainService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            try
            {
                // Преобразуем JSON в DTO
                var updatedTask = JsonSerializer.Deserialize<TaskDto>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (updatedTask == null)
                    return BadRequest("Invalid JSON format.");

                var jsonString = body.ToString();
                var jsonDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

                // Чтобы не нарушать связь, если userId не изменяется, просто берем то значение,
                // которое сейчас в задаче

                if (updatedTask.UserId == null)
                {
                    updatedTask.UserId = task.UserId;
                }

                // Устанавливаем значения null для соответствующих полей
                mainService.SetNullFields(task, jsonDict);

                // Если при изменении задачи было передано значение для projectId
                // не рассматривается ситуация, когда поле projectId = null, 
                // потому что отвязать задачу от проекта нельзя, можно только удалить на совсем
                if (jsonDict.ContainsKey("projectId"))
                {
                    if (jsonDict["projectId"] != null)
                    {
                        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == updatedTask.ProjectId);
                        // обновление списка задач, которые принадлежат к указанному проекту
                        if (project == null)
                            return BadRequest("Проект не существует");
                        else
                            if (project.TaskIds != null)
                            project.TaskIds.Add(id);
                        else
                            project.TaskIds = new List<int> { id };
                    }
                }
                // Если при изменении задачи было передано значение для projectId
                if (jsonDict.ContainsKey("eventId"))
                {
                    var evt = await _context.Events.FirstOrDefaultAsync(p => p.Id == updatedTask.EventId);
                    if (jsonDict["eventId"] != null)
                    {
                        // обновление списка задач, которые принадлежат к указанному проекту
                        if (evt == null)
                            return BadRequest("Событие не существует");
                        else
                            evt.TaskIds.Add(id);
                    }
                    else
                    {
                        if (evt == null)
                            return BadRequest("Событие не существует");
                        else
                            evt.TaskIds.Remove(id);
                    }
                }

                _mapper.Map(updatedTask, task);

                // Валидация
                mainService.ValidateEntityForNotNullConstraints(task);

                _context.Tasks.Update(task);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }

            return NoContent();
        }

        // DELETE: api/Task/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTask(int id, [FromServices] MainService taskService)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();
            try
            {
                await taskService.RemoveTask(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
