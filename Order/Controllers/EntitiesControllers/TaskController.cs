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
    [Authorize]
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
