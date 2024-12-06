using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.Models;
using Order.Services;
using System;

namespace Order.Controllers
{
    [Route("api/Schedule")]
    [ApiController]
    public class ShareController : ControllerBase
    {
        private readonly ScheduleSharingService _scheduleSharingService;
        private readonly ApplicationDbContext _context;

        public ShareController(ScheduleSharingService scheduleSharingService, ApplicationDbContext context)
        {
            _scheduleSharingService = scheduleSharingService;
            _context = context;
        }

        // Эндпоинт для проверки на то, что запрос был
        [HttpGet("{token}")]
        public async Task<IActionResult> ValidateShareLink(string token)
        {
            var sharedLink = await _context.ScheduleSharings
                .Where(link => link.PublicLinkToken == token)
                .FirstOrDefaultAsync();

            if (sharedLink == null)
            {
                return NotFound(new { message = "Link not found or expired." });
            }

            return Ok(new
            {
                sharedLink.UserId,
                StartTime = sharedLink.PeriodStart,
                EndTime = sharedLink.PeriodEnd
            });
        }

        [HttpPost("create-public-link")]
        public async Task<IActionResult> CreatePublicLink(Guid userId, DateTime periodStart, DateTime periodEnd)
        {
            var publicLinkToken = await _scheduleSharingService.CreatePublicLinkAsync(userId, periodStart, periodEnd);

            var publicLink = $"{Request.Scheme}://{Request.Host}/api/schedule/public/{publicLinkToken}";

            return Ok(new { publicLink });
        }

        // Непосредственно получение данных
        [HttpGet("public/{token}")]
        public async Task<IActionResult> GetScheduleByPublicLink(string token)
        {
            var schedule = await _context.ScheduleSharings
                .FirstOrDefaultAsync(s => s.PublicLinkToken == token);

            if (schedule == null)
            {
                return NotFound(new { message = "Schedule not found" });
            }

            var tasks = await _scheduleSharingService.GetTasksForPeriodAsync(schedule.UserId, schedule.PeriodStart, schedule.PeriodEnd);
            var events = await _scheduleSharingService.GetEventsForPeriodAsync(schedule.UserId, schedule.PeriodStart, schedule.PeriodEnd);

            return Ok(new { tasks, events });
        }
    }
}
