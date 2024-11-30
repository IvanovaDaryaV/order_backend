using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order;
using Order.Models;
//using Order.Services;
using System.Text.Json;

namespace Order.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CalendarController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Weekly/{userId}")]
        public async Task<IActionResult> GetWeeklyData(Guid userId, [FromQuery] string data)
        {
            if (_context == null)
                return StatusCode(500, "Database context is not initialized.");

            if (string.IsNullOrEmpty(data))
                return BadRequest("Date range is required.");

            var dateRange = System.Text.Json.JsonSerializer.Deserialize<string[]>(data);
            if (dateRange == null || dateRange.Length != 2)
                return BadRequest("Invalid date range format.");

            if (!DateOnly.TryParse(dateRange[0], out var startDate) ||
                !DateOnly.TryParse(dateRange[1], out var endDate))
            {
                return BadRequest("Invalid date format.");
            }

            if (startDate > endDate)
                return BadRequest("Start date cannot be later than end date.");

            // Получение данных пользователя
            var user = await _context.Users
                .Include(u => u.Tasks)
                .Include(u => u.Events)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound($"User with ID {userId} not found.");

            var filteredTasks = user.Tasks?
                                    .Where(task => task.HardDeadline >= startDate && task.HardDeadline <= endDate)
                                    .ToList();

            var filteredEvents = user.Events?
                .Where(evt => evt.CalendarDate >= startDate && evt.CalendarDate <= endDate)
                .ToList();

            if (user.Tasks == null || user.Events == null)
                return StatusCode(500, "Tasks or Events are null even after loading.");

            var contextIds = filteredTasks
                .Select(t => (int?)t.ContextId)
                .Concat(filteredEvents.Select(e => e.ContextId))
                .Distinct()
                .ToList();

            var contexts = await _context.Contexts
                //.Where(c => contextIds.Contains(c.Id))
                //.Select(c => c.Name)
                .Distinct()
                .ToListAsync();

            return Ok(new
            {
                user.Id,
                Tasks = filteredTasks,
                Events = filteredEvents,
                Contexts = contexts
            });
        }

        // создание ссылки на календарь
        [HttpPost("Calendar/Share")]
        public IActionResult ShareCalendar(int calendarId, [FromBody] ShareRequest request)
        {
            // Проверяем наличие календаря
            /*var calendar = calendarService.GetCalendarById(calendarId);
            if (calendar == null)
            {
                return NotFound("Calendar not found.");
            }*/

            // Проверяем корректность периода
            if (request.StartDate >= request.EndDate)
            {
                return BadRequest("StartDate must be earlier than EndDate.");
            }

            // Генерируем уникальный код ссылки
            var uniqueCode = Guid.NewGuid().ToString("N");

            // Сохраняем данные о ссылке в БД
            /*_sharedLinkRepository.Add(new SharedLink
            {
                CalendarId = calendarId,
                Code = uniqueCode,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = DateTime.UtcNow
            });*/

            // Возвращаем ссылку
            return Ok(new { ShareLink = $" http://localhost:5141/Calendar/Shared/{uniqueCode}" });
        }

        // просмотр по ссылке
        //[HttpGet("calendar/shared/{code}")]
        /*public IActionResult GetSharedCalendar(string code)
        {
            // Проверяем, существует ли ссылка
            var sharedLink = _sharedLinkRepository.GetByCode(code);
            if (sharedLink == null)
            {
                return NotFound("Invalid or expired link.");
            }

            // Получаем события из календаря за указанный период
            var events = _eventService.GetEventsByCalendarIdAndPeriod(
                sharedLink.CalendarId,
                sharedLink.StartDate,
                sharedLink.EndDate
            );

            // Убираем детали приватных событий
            var sanitizedEvents = events.Select(e => new
            {
                e.Id,
                e.Date,
                e.Duration,
                IsPrivate = e.IsPrivate,
                Details = e.IsPrivate ? "This time is busy" : e.Details
            });

            // Возвращаем данные
            return Ok(sanitizedEvents);
        }

    }
*/

        public class ShareRequest
        {
            public DateTime StartDate { get; set; } // Начало периода
            public DateTime EndDate { get; set; } // Конец периода
        }
        //public class SharedLink
        //{
        //    public int Id { get; set; }
        //    public int CalendarId { get; set; }
        //    public string Code { get; set; } // Уникальный код ссылки
        //    public DateTime StartDate { get; set; }
        //    public DateTime EndDate { get; set; }
        //    public DateTime CreatedAt { get; set; }
        //}
        //public interface ISharedLinkRepository
        //{
        //    void Add(SharedLink link);
        //    SharedLink GetByCode(string code);
        //}
    }
}
