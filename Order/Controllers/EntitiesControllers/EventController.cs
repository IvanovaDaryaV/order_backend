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
    [Authorize]
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
            var evt = await _context.Events.FindAsync(id);
            var tasks = _context.Tasks
                        .Where(task => evt.TaskIds.Contains(task.Id))
                        .ToList();
            if (evt == null)
                return NotFound();
            return Ok(new
            {
                evt,
                tasks
            });
        }

        // POST: api/Event
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, newEvent);
        }

        // PUT: api/Event/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] JsonElement body, [FromServices] MainService mainService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var evt = await _context.Events.Include(e => e.Tasks).FirstOrDefaultAsync(e => e.Id == id);
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

                // Проверка: было ли передано новое значение TaskIds
                if (jsonDict.ContainsKey("taskIds"))
                {
                    // Значение было передано и оно не null
                    if (jsonDict["taskIds"] != null)
                    {
                        var tasksToUpdate = await _context.Tasks
                            .Where(t => updatedEvent.TaskIds.Contains(t.Id))
                            .ToListAsync();

                        // Если количество найденных задач не совпадает с количеством переданных id
                        if (tasksToUpdate.Count != updatedEvent.TaskIds.Count)
                        {
                            return BadRequest("Некоторые из переданных задач не найдены.  Изменения не были применены.");
                        }
                        else
                        {
                            // Конфликт возникает, если у задачи из списка уже есть событие, к которому она привязана,
                            // и это событие не текущее.
                            // Если задача уже привязана к этому событию, конфликта не будет
                            // Например, чтобы дополнить список, не нужно переприсваивать значения заново
                            bool conflict = false;
                            foreach (var task in tasksToUpdate)
                            {
                                if (task.EventId != null && task.EventId != id)
                                    conflict = true;
                            }
                            if (!conflict)
                            {
                                await mainService.UnassignTasksFromEvent(id);
                                await mainService.AssignTasksToEvent(id, updatedEvent.TaskIds);
                            }
                            else
                            {
                                return BadRequest("Конфликт: задача(и) уже привязана к другому событию.  Изменения не были применены.");
                            }

                        }
                    }
                    // Если передано значение null
                    else
                    {
                        await mainService.UnassignTasksFromEvent(id);
                    }
                }
                // Если новых задач не было - без изменений
                else
                {
                    var taskIds = evt.Tasks.Select(t => t.Id).ToList();
                    updatedEvent.TaskIds = taskIds;
                }

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
