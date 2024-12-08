﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order;
using Order.Models;
//using Order.Services;
using System.Text.Json;

namespace Order.Controllers
{
    [ApiController]
    [Authorize]
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
                .Where(evt => evt.PeriodStart >= startDateTime && evt.PeriodStart <= endDateTime &&
                                evt.PeriodEnd >= startDateTime && evt.PeriodEnd <= endDateTime)
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

    }
}
