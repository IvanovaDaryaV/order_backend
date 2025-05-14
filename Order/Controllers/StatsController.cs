using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.Models;

/*
 Контроллер для аналитики (статистика по выполнению задач пользователя)
 */

namespace Order.Controllers
{
    public class StatsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public StatsController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Основной эндпоинт статистики
        [HttpGet("overview")]
        public async Task<IActionResult> GetStats(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound();

            var tasks = user.Tasks;
            if (tasks == null)
                return NotFound();

            var review = "this is review";

            // Получение статистики
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var tasksCompleted = tasks.Where(t => t.DateDone.HasValue).ToList();
            var overdueTasks = tasks.Where(t =>
                    !t.DateDone.HasValue &&
                    t.HardDeadline.HasValue &&
                    t.HardDeadline.Value < today
                );

            var avgDelay = tasksCompleted
                .Where(t => t.HardDeadline != null && t.DateDone != null)
                .Select(t => (t.DateDone.Value.Date - t.HardDeadline.Value.ToDateTime(TimeOnly.MinValue)).TotalHours)
                .DefaultIfEmpty()
                .Average();

            var completionRate = tasksCompleted
                .GroupBy(t => t.DateDone.Value.DayOfWeek)
                .ToDictionary(
                    g => g.Key.ToString(),
                    g => g.Count()
                );

            var result = new
            {
                TasksTotal = tasks.Count,
                TasksCompleted = tasksCompleted.Count,
                TasksOverdue = overdueTasks.Count(),
                AverageCompletionDelayHours = Math.Round(avgDelay, 2),
                CompletionRatePerDay = completionRate
            };

            return Ok(result);
        }

        [HttpGet("heatmap")]
        public async Task<IActionResult> GetHeatmap(Guid userId)
        {
            var user = await _context.Users
        .Include(u => u.Tasks)
        .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.Tasks == null)
                return NotFound();

            var tasksCompleted = user.Tasks.Where(t => t.DateDone.HasValue);

            // Инициализация карты: день недели -> часы -> счётчик
            var heatmap = Enum.GetValues<DayOfWeek>()
                .ToDictionary(
                    day => day.ToString(),
                    day => Enumerable.Range(0, 24).ToDictionary(hour => hour, hour => 0)
                );

            foreach (var task in tasksCompleted)
            {
                var dateDone = task.DateDone.Value;
                var day = dateDone.DayOfWeek.ToString();
                var hour = dateDone.Hour;

                heatmap[day][hour]++;
            }

            return Ok(heatmap);
        }

        // Динамика "завала" 
        //[HttpGet("overload-analysis")]
        //public async Task<IActionResult> GetOverloadAnalysis()
        //{

        //}

        //// Рекомендации
        //[HttpGet("recommendations")]
        //public async Task<IActionResult> GetRecommendations()
        //{

        //}
    }
}
