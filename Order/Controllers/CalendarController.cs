using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order;
using Order.Models;
using Order.Models.DTO;
using System.Linq;

//using Order.Services;
using System.Text.Json;

namespace Order.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/[controller]")]
    public class CalendarController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CalendarController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

            if (!DateTime.TryParse(dateRange[0], out var startDateTime) ||
                !DateTime.TryParse(dateRange[1], out var endDateTime))
            {
                return BadRequest("Invalid date format.");
            }

            if (!DateOnly.TryParse(dateRange[0], out var startDate) ||
                !DateOnly.TryParse(dateRange[1], out var endDate))
            {
                return BadRequest("Invalid date format.");
            }

            if (startDateTime > endDateTime)
                return BadRequest("Start date cannot be later than end date.");

            // Получение данных пользователя ==========================================
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
                .Where(evt => evt.PeriodStart >= startDateTime && evt.PeriodStart <= endDateTime &&
                                evt.PeriodEnd >= startDateTime && evt.PeriodEnd <= endDateTime)
                .ToList();

            if (user.Tasks == null || user.Events == null)
                return StatusCode(500, "Tasks or Events are null even after loading.");

            var contextIds = filteredTasks
                .Where(task => task.ContextId != null)
                .Select(task => task.ContextId)
                .Distinct()
                .ToList();

            //var contexts = await _context.Contexts
            //    //.Where(c => contextIds.Contains(c.Id))
            //    //.Select(c => c.Name)
            //    .Distinct()
            //    .ToListAsync();

            var contexts = await _context.Contexts
                .Where(c => contextIds.Contains(c.Id))
                .ToListAsync();

            // Маппируем контексты на ContextDto
            //var contextDtos = _mapper.Map<List<ContextDto>>(contexts);
            var contextDtos = _context.Contexts
                .Where(c => contextIds.Contains(c.Id))
                .Select(c => new ContextDto
                {
                    //Id = c.Id,
                    Name = c.Name,
                    Place = c.Place,
                    UserId = c.UserId
                })
                .ToList();

            return Ok(new
            {
                user.Id,
                Tasks = filteredTasks,
                Events = filteredEvents,
                Contexts = contextDtos
            });
        }

    }
}
