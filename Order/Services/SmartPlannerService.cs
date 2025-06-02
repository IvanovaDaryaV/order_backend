using Order.Models;

namespace Order.Services
{
    public class SmartPlannerService
    {
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
    }
}
