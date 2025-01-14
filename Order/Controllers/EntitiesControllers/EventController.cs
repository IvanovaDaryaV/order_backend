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


            // Преобразуем JSON в DTO
            var updatedEvent = JsonSerializer.Deserialize<EventDto>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (updatedEvent == null)
                return BadRequest("Invalid JSON format.");

            var jsonString = body.ToString();
            var jsonDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

            try
            {
                // Устанавливаем значения null для соответствующих полей
                mainService.SetNullFields(evt, jsonDict);
            


                // Если переданы новые задачи, привязываем их
                if (updatedEvent.TaskIds != null && updatedEvent.TaskIds.Any())
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
                        await mainService.UnassignTasksFromEvent(id);
                        foreach (var task in tasksToUpdate)
                        {
                            await mainService.AssignTasksToEvent(id, updatedEvent.TaskIds);
                        }

                    }

                }

                // Если при изменении объекта события не были переданы задачи,
                // список остается без изменений. Если не сделать это вручную,
                // поле занулится
                else
                {
                    var taskIds = evt.Tasks.Select(t => t.Id).ToList();
                    updatedEvent.TaskIds = taskIds;
                }

                // Чтобы не нарушать связь, если userId не изменяется, просто берем то значение, которое уже есть

                if (updatedEvent.UserId == null)
                {
                    updatedEvent.UserId = evt.UserId;
                }

                _mapper.Map(updatedEvent, evt);

                // Валидация
                mainService.ValidateEntityForNotNullConstraints(evt);

                _context.Events.Update(evt);
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
