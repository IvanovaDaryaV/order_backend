using Microsoft.EntityFrameworkCore;
using Order.Models;

namespace Order.Services
{
    public class ScheduleSharingService
    {
        private readonly ApplicationDbContext _context;

        public ScheduleSharingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreatePublicLinkAsync(Guid userId, DateTime periodStart, DateTime periodEnd)
        {
            // Генерация уникального токена
            var publicLinkToken = Guid.NewGuid().ToString();

            var schedule = new ScheduleSharing
            {
                UserId = userId,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                PublicLinkToken = publicLinkToken
            };

            _context.ScheduleSharings.Add(schedule);
            await _context.SaveChangesAsync();

            return publicLinkToken;
        }

        public async Task<bool> IsScheduleSharedWithPublicLinkAsync(string publicLinkToken)
        {
            var schedule = await _context.ScheduleSharings
                .FirstOrDefaultAsync(s => s.PublicLinkToken == publicLinkToken);

            return schedule != null;
        }

        public async Task<List<Models.Task>> GetTasksForPeriodAsync(Guid userId, DateTime periodStart, DateTime periodEnd)
        {
            // Логика для получения задач в определенный период
            return await _context.Tasks
                .Where(t => t.UserId == userId && t.CallendarDate >= DateOnly.FromDateTime(periodStart) && t.CallendarDate <= DateOnly.FromDateTime(periodEnd))
                .ToListAsync();
            
        }

        public async Task<List<Event>> GetEventsForPeriodAsync(Guid userId, DateTime periodStart, DateTime periodEnd)
        {
            // Логика для получения событий в определенный период
            return await _context.Events
                .Where(evt => evt.PeriodStart >= periodStart && evt.PeriodStart <= periodEnd &&
                                evt.PeriodEnd >= periodStart && evt.PeriodEnd <= periodEnd)
                .ToListAsync();
        }
    }
}
