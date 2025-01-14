using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.Models;
using Order.Models.DTO;
using System.Text.Json;

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

        [HttpGet("inbox/{userId:Guid}")]
        public async Task<IActionResult> GetInboxTasks(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var inboxTasks = await _context.Tasks
                .Where(task => task.UserId == user.Id &&
                task.ContextId == null && task.ProjectId == null && task.EventId == null && task.CalendarDate == null)
                .ToListAsync();

            return Ok(inboxTasks);
        }

        // POST: api/Task
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] Models.Task newTask, [FromServices] MainService taskService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // если при создании задачи указано, что она относится к какому-то определенному проекту
            //if (newTask.ProjectId != null)
            //    await taskService.AssignTasksToProject((int)newTask.ProjectId, new List<int> (newTask.Id));

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

            // Преобразуем JSON в DTO
            var updatedTask = JsonSerializer.Deserialize<TaskDto>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (updatedTask == null)
                return BadRequest("Invalid JSON format.");

            var jsonString = body.ToString();
            var jsonDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

            try
            {
                // Устанавливаем значения null для соответствующих полей
                mainService.SetNullFields(task, jsonDict);
                // Чтобы не нарушать связь, если userId не изменяется, просто берем то значение,
                // которое сейчас в задаче

                if (updatedTask.UserId == null)
                {
                    updatedTask.UserId = task.UserId;
                }


                _mapper.Map(updatedTask, task);

                // Валидация
                mainService.ValidateEntityForNotNullConstraints(task);

                _context.Tasks.Update(task);
                await _context.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                // Возвращаем ошибку, если поле не допускает null
                return BadRequest(new { error = ex.Message });
            }
            catch (DbUpdateException dbEx)
            {
                // Перехватываем исключение уровня базы данных
                return BadRequest(new { error = "Ошибка базы данных", details = dbEx.Message });
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
