using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.Models;
using System.Text;
using Microsoft.ML;
using Microsoft.ML.Data;
using Order.Services;
using System.Threading.Tasks;
using Ical.Net.CalendarComponents;
using System.Net.Http;
using Org.BouncyCastle.Asn1;

/*
 Контроллер для аналитики (статистика по выполнению задач пользователя)
 */

namespace Order.Controllers
{
    public class StatsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly HttpClient _httpClient;

        public StatsController(ApplicationDbContext context, IMapper mapper, HttpClient httpClient)
        {
            _context = context;
            _mapper = mapper;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:8000");
        }

        // Основной эндпоинт общей статистики
        [HttpGet("/api/overview")]
        public async Task<IActionResult> GetStats(Guid userId)
        {
            ICollection<Models.Task> tasks;
            try
            {
                var user = await _context.Users
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.UserId == userId);
                tasks = user.Tasks;
            }
            catch(NullReferenceException)
            {
                return Ok(new { });
            }
            if (tasks.Count != 0)
            {
                // Получение статистики
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                //var tasksCompleted = tasks.Where(t => t.DateDone.HasValue).ToList();
                var tasksCompleted = tasks.Where(t => t.Status == true).ToList();
                var overdueTasks = tasks.Where(t =>
                        //!t.DateDone.HasValue &&
                        t.Status == false &&
                        t.HardDeadline.HasValue &&
                        t.HardDeadline.Value < today
                    );

                var avgDelay = tasksCompleted
                    //.Where(t => t.HardDeadline != null && t.DateDone != null)
                    .Where(t => t.Status == true)
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
            else
            {
                //return Ok(new
                //{
                //    TasksTotal = 0,
                //    TasksCompleted = 0,
                //    TasksOverdue = 0,
                //    AverageCompletionDelayHours = 0,
                //    CompletionRatePerDay = 0
                //});
                return Ok(new { });
            }
            
        }

        [HttpGet("/api/heatmap")]
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

        // Рекомендации
        [HttpGet("/api/recommendations")]
        public async Task<IActionResult> GetRecommendations(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null || user.Tasks == null)
                return NotFound();

            var tasks = user.Tasks;
            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);
            var weekAgo = now.AddDays(-7);
            var weekAhead = now.AddDays(7);

            // Выполненные задачи за последнюю неделю
            var completed = tasks
                .Where(t => t.DateDone.HasValue && t.DateDone.Value >= weekAgo)
                .ToList();

            double avgCompletedPerDay = completed.Count / 7.0;

            // Просроченные задачи (не помечены как выполненные)
            var overdue = tasks
                .Where(t => !t.DateDone.HasValue && t.HardDeadline.HasValue && t.HardDeadline.Value < today)
                .ToList();

            // Задачи, которые предстоит выполнить в ближайшую неделю
            var upcoming = tasks
                .Where(t => !t.DateDone.HasValue &&
                            t.HardDeadline.HasValue &&
                            t.HardDeadline.Value >= today &&
                            t.HardDeadline.Value <= DateOnly.FromDateTime(weekAhead))
                .ToList();

            // Формирование рекомендаций
            var sb = new StringBuilder();
            //sb.AppendLine($"Вы в среднем завершаете {Math.Round(avgCompletedPerDay, 2)} задач в день.");

            if (overdue.Count > 0)
            {
                var avgDelay = overdue
                    .Select(t => today.DayNumber - t.HardDeadline.Value.DayNumber)
                    .Average();

                sb.AppendLine($"У вас {overdue.Count} просроченных задач, в среднем на {Math.Round(avgDelay, 1)} дней.");
                if (avgDelay > 2)
                    sb.AppendLine("Рекомендуется срочно закрыть самые старые задачи.");
            }
            else
            {
                sb.AppendLine("У вас нет просроченных задач — отлично!");
            }

            sb.AppendLine($"В течение следующей недели предстоит выполнить {upcoming.Count} задач.");

            if (avgCompletedPerDay == 0)
            {
                sb.AppendLine("Вы не завершали задачи на этой неделе. Попробуйте начать с простых задач.");
            }
            else if (upcoming.Count > avgCompletedPerDay * 7)
            {
                sb.AppendLine("Похоже, объём задач превышает вашу текущую продуктивность. Перераспределите нагрузку.");
            }
            else
            {
                sb.AppendLine("Нагрузка на следующую неделю соответствует вашему темпу — продолжайте в том же духе!");
            }

            // Совет в конце ?
            sb.AppendLine();
            sb.AppendLine("Совет: планируйте не более 3 приоритетных задач в день.");

            return Ok(new
            {
                Recommendations = sb.ToString()
            });
        }

        [HttpGet("/api/intellectual-planning")]
        public async Task<IActionResult> GetPlan(Guid userId, [FromServices] StatisticsService plannerService)
        {
            var user = await _context.Users
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return NotFound();

            var tasks = user.Tasks.ToList();
            if (tasks == null)
                return NotFound();

            var completedTasks = tasks.Where(t => t.DateDone != null);

            // Подсчет среднего времени выполнения задач пользователем
            var avgCompletionTime = plannerService.CalculateAverageCompletionTime(completedTasks.ToList());

            // Подсчет оптимального времени начала для каждой задачи
            string tmp = string.Empty;
            //List<DateOnly> dates = new List<DateOnly>();
            //List<int> taskIds = new List<int>();

            //Dictionary<int, DateOnly> tasksDates = new Dictionary<int, DateOnly>();

            var tasksDates = plannerService.CalculateStartDate(tasks, avgCompletionTime);

            //foreach (Models.Task task in tasks)
            //{
            //    if (task.HardDeadline != null && task.DateDone == null)
            //    {
            //        tasksDates.Add(task.TaskId, plannerService.CalculateStartDate(task, avgCompletionTime));

            //        //tmp += $"\n{task.Name}: " +
            //        //    $"{date} " +
            //        //    $"(жесткий дедлайн: {task.HardDeadline}, приоритет задачи: {(task.Priority == null ? 0 : task.Priority)}) " +
            //        //    $"итого нужно начать за {task.HardDeadline.Value.DayNumber - date.DayNumber} дней";
            //    }
            //}

            //return Ok($"Среднее время выполнения задач для пользователя {userId}: {avgCompletionTime.ToString().Split('.')[0]} дней, {avgCompletionTime.ToString().Split('.')[1]} часов. " +
            //    $"\n\n" +
            //    $"Рекомендованные даты для начала задач: {tmp}");
            
            return Ok(new
            {
                UserId = userId,
                AvgDays = avgCompletionTime.ToString().Split('.')[0],
                AvgHours = avgCompletionTime.ToString().Split('.')[1],
                DatesForTaskIds = tasksDates
            });
        }

       
        //[HttpGet("distribute-tasks")]
        //public async Task<IActionResult> GetPlan2(Guid userId, [FromServices] SmartPlannerService plannerService)
        //{
        //    var user = await _context.Users
        //        .Include(u => u.Tasks)
        //        .FirstOrDefaultAsync(u => u.UserId == userId);
        //    if (user == null)
        //        return NotFound();

        //    var tasks = user.Tasks;
        //    if (tasks == null)
        //        return NotFound();

        //    var completedTasks = tasks.Where(t => t.DateDone != null);

        //    // Подсчет среднего времени выполнения задач пользователем
        //    var avgCompletionTime = plannerService.CalculateAverageCompletionTime(completedTasks.ToList());

        //    // Составление оптимального расписания для пользователя
        //    return Ok(plannerService.DistributeTasks(tasks.ToList(), avgCompletionTime));
        //}

        // Вычисление риска просрочки задачи
        //[HttpGet("/api/risk")]
        //public async Task<IActionResult> GetTaskRisk(Guid userId, int taskId, [FromServices] StatisticsService statService)
        //{
        //    var task = await _context.Tasks
        //        .FirstOrDefaultAsync(t => t.TaskId == taskId);

        //    var risk = await statService.GetOverdueProbability(task, userId);

        //    return Ok(new { taskId, riskScore = risk });
        //}


        // Вычисление возможных завалов на 30 дней вперед
        [HttpGet("/api/overload-prediction-ML")]
        public async Task<IActionResult> GetOverloadPrediction(Guid userId, string targetDateString, [FromServices] StatisticsService statService)
        {
            var user = await _context.Users
               .Include(u => u.Tasks)
               .FirstOrDefaultAsync(u => u.UserId == userId);
            var tasks = user.Tasks.ToList();

            if (!DateOnly.TryParseExact(targetDateString, "dd.MM.yyyy", out var targetDate))
                return BadRequest("Неверный формат даты. Используйте dd.MM.yyyy");
            //Console.WriteLine(targetDate);

            try
            {
                var risk = await statService.GetOverloadPrediction(tasks, targetDate);

                return Ok(new { riskScore = risk });
            }
            catch (Exception ex) { 
                return BadRequest("Прежде чем получить предсказание, необходимо обучить модель. Отправьте запрос на эндпоинт /api/fit-model-ML");
            }
            
        }

        // Метод для генерации данных и обучения модели
        [HttpGet("/api/fit-model-ML")]
        public async Task<IActionResult> fitModel()
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/train-model", new { });
                var rawJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine("RAW JSON:");
                Console.WriteLine(rawJson);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
