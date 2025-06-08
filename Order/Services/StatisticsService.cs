using AutoMapper;
using Ical.Net.CalendarComponents;
using MailKit;
using Microsoft.AspNetCore.Mvc;
using Order.Models;
using Order.Models.Forecast;
using System.Net.Http;
using System.Threading.Tasks;

namespace Order.Services
{
    public class StatisticsService
    {
        //private readonly TaskForecastService _forecastService;

        //public StatisticsService(TaskForecastService forecastService)
        //{
        //    _forecastService = forecastService;
        //}

        private readonly HttpClient _httpClient;

        public StatisticsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://localhost:8000");
        }

        // ВЫЧИСЛЕНИЕ ОПТИМАЛЬНОЙ ДАТЫ ДЛЯ НАЧАЛА ВЫПОЛНЕНИЯ ЗАДАЧИ ========================================
        public DateOnly CalculateStartDate(Models.Task task, TimeSpan avgCompletionTime)
        {
            // Базовый буфер: 1.5 * среднее время выполнения
            double bufferDays = avgCompletionTime.TotalDays * 1.5;

            // Корректировка на приоритет (чем выше, тем раньше начинаем)
            // 1 - высокий приоритет, 3 - низкий
            if (task.Priority != null)
            {
                bufferDays *= (double)((4 - task.Priority) * 0.3); // Коэффициент
            }

            // Учет прокрастинации пользователя (+20% если часто откладывает)
            //if (avgDelayTime > 2)
            //    bufferDays *= 1.2;

            return task.HardDeadline.Value.AddDays((int)-bufferDays);
        }

        // Метод распределения задач по календарю без перегрузки
        public Dictionary<DateOnly, List<Models.Task>> DistributeTasks(List<Models.Task> tasks, TimeSpan avgCompletionTime)
        {
            var schedule = new Dictionary<DateOnly, List<Models.Task>>();
            double maxDailyTasks = 3; // Максимум 3 задачи в день (например)

            // Сортируем по приоритету и дедлайну
            var sortedTasks = tasks.OrderBy(t => t.Priority)
                                  .ThenBy(t => t.HardDeadline);

            foreach (var task in sortedTasks)
            {
                // Начинаем с сегодняшнего дня или дедлайна - буфер
                var startDay = DateOnly.FromDateTime(DateTime.Today);
                var deadlineDay = task.HardDeadline;

                for (var day = startDay; day <= deadlineDay; day = day.AddDays(1))
                {
                    if (!schedule.ContainsKey(day))
                        schedule[day] = new List<Models.Task>();

                    // Проверяем, не превышен ли дневной лимит
                    if (schedule[day].Count < maxDailyTasks)
                    {
                        schedule[day].Add(task);
                        break; // Переходим к следующей задаче
                    }
                }
            }

            // Удаляем пустые дни (если нужно)
            return schedule.Where(p => p.Value.Any())
                          .ToDictionary(p => p.Key, p => p.Value);
        }


        // Метод подсчета среднего выполнения задач для пользователя
        // Поля даты создания и даты выполнения необязательные в БД
        // Поэтому учитываются только те задачи, у которых эти поля заполнены
        public TimeSpan CalculateAverageCompletionTime(List<Models.Task> completedTasks)
        {
            if (completedTasks.Count == 0)
                return TimeSpan.Zero; // или значение по умолчанию

            var totalTime = TimeSpan.Zero;

            foreach (var task in completedTasks)
            {
                if (task.DateDone != null && task.DateCreated != null)
                {
                    totalTime += task.DateDone.Value - task.DateCreated.Value;
                }
            }

            return TimeSpan.FromTicks(totalTime.Ticks / completedTasks.Count);
        }

        //public async Task<object> GetUserStatistics(Guid userId)
        //{
        //    // 1. Получение исторических данных
        //    var history = await _forecastService.GetTaskHistoryAsync(userId);

        //    // 2. Подготовка данных для обучения
        //    var trainingData = history.GroupBy(x => x.Date.Date)
        //                             .Select(g => new TaskHistoryRecord
        //                             {
        //                                 Date = g.Key,
        //                                 TasksCompleted = g.Count()
        //                             })
        //                             .OrderBy(x => x.Date)
        //                             .ToList();

        //    // 3. Обучение модели (можно кэшировать)
        //    _forecastService.TrainModel(trainingData);

        //    // 4. Получение прогноза
        //    var forecast = _forecastService.Predict();

        //    return new UserStatistics
        //    {
        //        // ... существующие поля
        //        Forecast = forecast,
        //        ForecastAccuracy = CalculateAccuracy(history, forecast)
        //    };
        //}

        public async Task<string> GetOverdueProbability(Models.Task task, Guid userId)
        {
            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);
            var request = new
            {
                user_id = userId,
                task_created = task.DateCreated?.ToString("yyyy-MM-ddTHH:mm:ss"),
                task_deadline = task.HardDeadline?.ToString("yyyy-MM-dd"),
                task_completed = task.Status,
                priority = (int)task.Priority
            };
            Console.WriteLine(request);
            var response = await _httpClient.PostAsJsonAsync("/predict", request);
            //Console.WriteLine(response);

            var rawJson = await response.Content.ReadAsStringAsync();
            Console.WriteLine("RAW JSON ОТВЕТ:");
            Console.WriteLine(rawJson);

            var result = await response.Content.ReadFromJsonAsync<Dictionary<string, double>>();
            return $"Вероятность просрочки задачи: {result["probability"]}";

            //var user = await _context.Users
            //    .Include(u => u.Tasks)
            //    .FirstOrDefaultAsync(u => u.UserId == userId);
            //if (user == null)
            //    return NotFound();

            //var tasks = user.Tasks;
            //var task = await _context.Tasks
            //    .FirstOrDefaultAsync(t => t.TaskId == taskId);
            //var now = DateTime.UtcNow;
            //var today = DateOnly.FromDateTime(now);

            //if (task == null)
            //    return NotFound();

            //var overdueTasks = tasks.Where(t =>
            //        !t.DateDone.HasValue &&
            //        t.HardDeadline.HasValue &&
            //        t.HardDeadline.Value < today
            //    );

            //using var client = new HttpClient();

            //var response = await client.PostAsJsonAsync(
            //    "http://python-server:8000/predict",
            //    new
            //    {
            //        task_created = task.DateCreated.Value.ToString("yyyy-MM-dd"),
            //        task_deadline = task.HardDeadline.Value.ToString("yyyy-MM-dd"),
            //        priority = (int)task.Priority,
            //        user_past_overdue_rate = overdueTasks.Count() / tasks.Count()
            //    });

            //var result = await response.Content.ReadFromJsonAsync<Dictionary<string, double>>();
            //return Ok(result["overdue_probability"]);
        }

    }
}
