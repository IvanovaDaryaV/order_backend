using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order;
using Order.Models;
using Order.Models.DTO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Order.Controllers.EntitiesControllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[Authorize]
    public class EventController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public EventController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Event/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var evt = await _context.Events
                        .Include(evt => evt.Tasks)
                        .FirstOrDefaultAsync(evt => evt.EventId == id);
            if (evt == null)
                return NotFound();

            return Ok(evt);
        }

        // POST: api/Event
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.EventId }, newEvent);
        }

        // PUT: api/Event/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] JsonElement body, [FromServices] MainService mainService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var evt = await _context.Events.Include(e => e.Tasks).FirstOrDefaultAsync(e => e.EventId == id);
            if (evt == null)
                return NotFound();

            try
            {
                // Преобразуем JSON в DTO
                var updatedEvent = JsonSerializer.Deserialize<EventDto>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (updatedEvent == null)
                    return BadRequest("Invalid JSON format.");

                var jsonString = body.ToString();
                var jsonDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

                // Чтобы не нарушать связь, если userId не изменяется, просто берем то значение, которое уже есть

                if (updatedEvent.UserId == null)
                {
                    updatedEvent.UserId = evt.UserId;
                }

                // Устанавливаем значения null для соответствующих полей
                mainService.SetNullFields(evt, jsonDict);

                _mapper.Map(updatedEvent, evt);

                // Валидация
                mainService.ValidateEntityForNotNullConstraints(evt);

                _context.Events.Update(evt);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            return NoContent();

        }

        // Метод привязки задач к событию (чтобы не передавать полностью объекты задач - только список id)
        [HttpPost("{eventId}/assign-tasks")]
        public async Task<IActionResult> AssignTasksToEvent(int eventId, [FromBody] List<int> taskIds)
        {
            var evt = await _context.Events.FindAsync(eventId);
            if (evt == null)
                return NotFound("Событие не найдено");

            var tasks = await _context.Tasks
                .Where(t => taskIds.Contains(t.TaskId))
                .ToListAsync();

            // Проверка на конфликты
            var conflictTasks = tasks.Where(t => t.EventId != null && t.EventId != eventId).ToList();
            if (conflictTasks.Any())
            {
                return BadRequest($"Некоторые задачи уже привязаны к другому событию: {string.Join(", ", conflictTasks.Select(t => t.TaskId))}");
            }

            // Привязка задач
            foreach (var task in tasks)
            {
                task.EventId = eventId;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // Метод отвязки задач от события (чтобы не передавать полностью объекты задач - только список id)
        [HttpPost("{eventId}/unassign-tasks")]
        public async Task<IActionResult> UnassignTasksFromEvent(int eventId, [FromBody] List<int> taskIds)
        {
            var evt = await _context.Events.FindAsync(eventId);
            if (evt == null)
                return NotFound("Событие не найдено");

            var tasks = await _context.Tasks
                .Where(t => taskIds.Contains(t.TaskId) && t.EventId == eventId)
                .ToListAsync();

            if (tasks.Count == 0)
                return BadRequest("Не найдены задачи, привязанные к указанному событию");

            foreach (var task in tasks)
            {
                task.EventId = null;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // DELETE: api/Event/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt == null)
                return NotFound();

            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
