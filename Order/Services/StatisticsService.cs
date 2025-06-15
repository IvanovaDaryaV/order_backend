using AutoMapper;
using Ical.Net.CalendarComponents;
using MailKit;
using Microsoft.AspNetCore.Mvc;
using Order.Models;
using Org.BouncyCastle.Asn1.Ocsp;
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
        public (Dictionary<int, DateOnly> TasksDates, List<int> FailedToSchedule) CalculateStartDate(List<Models.Task> tasks, TimeSpan avgCompletionTime)
        {
            double bufferDays = avgCompletionTime.TotalDays * 1.5;
            int max_tasks_per_day = 5;

            Dictionary<int, DateOnly> tasksDates = new Dictionary<int, DateOnly>();
            Dictionary<DateOnly, int> dateTaskCounts = new Dictionary<DateOnly, int>();
            List<int> failedToSchedule = new List<int>();

            var today = DateOnly.FromDateTime(DateTime.Today);

            foreach (Models.Task task in tasks)
            {
                DateOnly finalDate;

                if (task.HardDeadline != null)
                {
                    // Если у задачи есть срок, но он уже прошел, то не можем предложить оптимальную дату начала
                    if (task.HardDeadline < today) {
                        failedToSchedule.Add(task.TaskId);
                    }
                    // У задачи есть срок и он позже сегодняшнего дня
                    else
                    {
                        double adjustedBufferDays = bufferDays;

                        if (task.Priority != null)
                        {
                            adjustedBufferDays *= (double)((4 - task.Priority) * 0.3);
                        }

                        // Рассчитываем предполагаемую дату начала с учётом буфера
                        finalDate = task.HardDeadline.Value.AddDays((int)-adjustedBufferDays);

                        // если вычисленная дата уже прошла, то выставляем сегодняшнюю
                        if (finalDate < today)
                        {
                            bool failedToScheduleFlag = false;
                            finalDate = today;

                            // ищем подходящую дату дальше во времени
                            // прибавляем по одному дню, пока не найдем свободный
                            while (dateTaskCounts.ContainsKey(finalDate) && dateTaskCounts[finalDate] >= max_tasks_per_day)
                            {
                                finalDate = finalDate.AddDays(1);

                                // если свободного дня до дедлайна нет, то запланировать не получится
                                if (finalDate > task.HardDeadline.Value)
                                {
                                    failedToSchedule.Add(task.TaskId);
                                    finalDate = default;
                                    failedToScheduleFlag = true;
                                    break;
                                }
                            }

                            if (!failedToScheduleFlag)
                            {
                                tasksDates[task.TaskId] = finalDate;

                                // повышаем счетчик кол-ва задач
                                if (dateTaskCounts.ContainsKey(finalDate))
                                    dateTaskCounts[finalDate]++;
                                else
                                    dateTaskCounts[finalDate] = 1;
                            }
                        }

                        // если finalDate > today && finalDate < hardDeadline
                        else
                        {
                            DateOnly originalStart = finalDate;
                            bool successPlanningFlag = true;

                            // Ищем ближайший свободный день, отсчитывая назад
                            while (dateTaskCounts.ContainsKey(finalDate) && dateTaskCounts[finalDate] >= max_tasks_per_day)
                            {
                                finalDate = finalDate.AddDays(-1);

                                // если превысили дедлайн — не удалось запланировать
                                //if (finalDate > task.HardDeadline.Value)

                                // если дошли до сегодняшней даты - не удалось запланировать
                                if (finalDate < today)
                                {
                                    successPlanningFlag = false;
                                    break;
                                }
                            }

                            // если при отсчете назад не удалось запланировать, идем вперед к дедлайну
                            if (!successPlanningFlag)
                            {
                                while (dateTaskCounts.ContainsKey(finalDate) && dateTaskCounts[finalDate] >= max_tasks_per_day)
                                {
                                    finalDate = finalDate.AddDays(1);

                                    // если превысили дедлайн — не удалось запланировать
                                    if (finalDate > task.HardDeadline.Value)
                                    {
                                        failedToSchedule.Add(task.TaskId);
                                        finalDate = default;
                                        successPlanningFlag = false;
                                        break;
                                    }
                                }
                            }

                            if (successPlanningFlag)
                            {
                                tasksDates[task.TaskId] = finalDate;
                                if (dateTaskCounts.ContainsKey(finalDate))
                                    dateTaskCounts[finalDate]++;
                                else
                                    dateTaskCounts[finalDate] = 1;
                            }
                        }
                        
                    }
                }
                else
                {
                    // Нет дедлайна — ищем первый свободный день от today
                    finalDate = today;

                    while (dateTaskCounts.ContainsKey(finalDate) && dateTaskCounts[finalDate] >= max_tasks_per_day)
                    {
                        finalDate = finalDate.AddDays(1);
                    }

                    tasksDates[task.TaskId] = finalDate;

                    // повышаем счетчик кол-ва задач
                    if (dateTaskCounts.ContainsKey(finalDate))
                        dateTaskCounts[finalDate]++;
                    else
                        dateTaskCounts[finalDate] = 1;
                }
            }

            return (tasksDates, failedToSchedule);
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
        }

        // Метод для отправки запроса на питон для получения прогноза завала
        // Прогноз получается для каждого дня на 7 дней вперед от переданной targetDate
        public async Task<Dictionary<DateOnly, double>> GetOverloadPrediction(List<Models.Task> tasks, DateOnly startDate)
        {
            var resultDict = new Dictionary<DateOnly, double>();

            for (int i = 0; i < 8; i++)
            {
                var targetDate = startDate.AddDays(i);
                var prev3Days = targetDate.AddDays(-3);

                // кол-во просроченных задач за 3 дня до даты
                var overdueLast3d = tasks.Count(t =>
                    t.DateDone.HasValue &&
                    t.HardDeadline.HasValue &&
                    DateOnly.FromDateTime(t.DateDone.Value) > t.HardDeadline &&
                    DateOnly.FromDateTime(t.DateDone.Value) >= prev3Days &&
                    DateOnly.FromDateTime(t.DateDone.Value) < targetDate);

                // кол-во задач с высоким приоритетом в эту дату
                var highPriority = tasks.Count(t =>
                    t.DateCreated.HasValue &&
                    DateOnly.FromDateTime(t.DateCreated.Value) == targetDate &&
                    t.Priority >= 3);

                // ср вр выполнения задач 
                var completedTasks = tasks
                    .Where(t => t.DateDone.HasValue && t.DateCreated.HasValue)
                    .ToList();

                double avgDuration = completedTasks.Any()
                    ? completedTasks.Average(t => (t.DateDone.Value - t.DateCreated.Value).TotalHours)
                    : 0.0;

                // кол-во активных задач, созданных когда-либо до этой даты
                var activeTasks = tasks.Count(t =>
                    t.Status != true &&
                    t.DateCreated.HasValue &&
                    DateOnly.FromDateTime(t.DateCreated.Value) <= targetDate);

                // все задачи, дедлайн которых - эта дата
                var total_tasks = tasks.Count(t =>
                        t.HardDeadline != null &&
                        t.HardDeadline == targetDate);

                var request = new
                {
                    overdue_last_3d = overdueLast3d,
                    high_priority_due = highPriority,
                    total_tasks_due = total_tasks,
                    avg_total = Math.Round(avgDuration, 2),
                    active_tasks = activeTasks
                };

                Console.WriteLine($"Запрос на дату {targetDate}:");
                Console.WriteLine(request);

                var response = await _httpClient.PostAsJsonAsync("/predict", request);
                var rawJson = await response.Content.ReadAsStringAsync();
                Console.WriteLine("RAW JSON:");
                Console.WriteLine(rawJson);

                var result = await response.Content.ReadFromJsonAsync<Dictionary<string, double>>();
                resultDict[targetDate] = result["probability_zaval"];
            }

            return resultDict;
        }

    }
}
