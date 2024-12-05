using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.Models;
using Order.Services;

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

        //[HttpGet("check/{userId}/{periodStart}/{periodEnd}")]
        //public async Task<IActionResult> CheckScheduleSharing(string publicLinkToken)
        //{
        //    bool isShared = await _scheduleSharingService.IsScheduleSharedWithPublicLinkAsync(publicLinkToken);

        //    if (!isShared)
        //    {
        //        return Unauthorized(new { message = "Schedule is not shared for this period" });
        //    }

        //    var tasks = await _scheduleSharingService.GetTasksForPeriodAsync(userId, periodStart, periodEnd);
        //    var events = await _scheduleSharingService.GetEventsForPeriodAsync(userId, periodStart, periodEnd);

        //    return Ok(new { tasks, events });
        //}
        

        [HttpPost("create-public-link")]
        public async Task<IActionResult> CreatePublicLink(Guid userId, DateTime periodStart, DateTime periodEnd)
        {
            var publicLinkToken = await _scheduleSharingService.CreatePublicLinkAsync(userId, periodStart, periodEnd);

            var publicLink = $"{Request.Scheme}://{Request.Host}/api/schedule/public/{publicLinkToken}";

            return Ok(new { publicLink });
        }

        [HttpGet("public/{publicLinkToken}")]
        public async Task<IActionResult> GetScheduleByPublicLink(string publicLinkToken)
        {
            var isShared = await _scheduleSharingService.IsScheduleSharedWithPublicLinkAsync(publicLinkToken);

            if (!isShared)
            {
                return Unauthorized(new { message = "Invalid or expired public link" });
            }

            // Получаем задачи и события для расписания, ассоциированного с публичной ссылкой
            var schedule = await _context.ScheduleSharings
                .FirstOrDefaultAsync(s => s.PublicLinkToken == publicLinkToken);

            if (schedule == null)
            {
                return NotFound(new { message = "Schedule not found" });
            }

            var tasks = await _scheduleSharingService.GetTasksForPeriodAsync(schedule.UserId, schedule.PeriodStart, schedule.PeriodEnd);
            var events = await _scheduleSharingService.GetEventsForPeriodAsync(schedule.UserId, schedule.PeriodStart, schedule.PeriodEnd);

            return Ok(new { tasks, events });
        }



        //[HttpPost("calendar/share")]
        //public IActionResult ShareCalendar(int calendarId, [FromBody] ShareRequest request)
        //{
        //    if (request.StartDate >= request.EndDate)
        //    {
        //        return BadRequest("StartDate must be earlier than EndDate.");
        //    }

        //    // генерация уникального кода ссылки
        //    var uniqueCode = Guid.NewGuid().ToString("N");

        //    // Сохраняем данные о ссылке в БД
        //    //_sharedLinkRepository.Add(new SharedLink
        //    //{
        //    //    CalendarId = calendarId,
        //    //    Code = uniqueCode,
        //    //    StartDate = request.StartDate,
        //    //    EndDate = request.EndDate,
        //    //    CreatedAt = DateTime.UtcNow
        //    //});

        //    // Возвращаем ссылку
        //    return Ok(new { ShareLink = $"https://myapp.com/calendar/shared/{uniqueCode}" });
        //}

    }
}
