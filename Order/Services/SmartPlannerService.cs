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
            //if (AvgDelayDays > 2)
            //    bufferDays *= 1.2;

            return task.HardDeadline.Value.AddDays((int)-bufferDays);
        }

            //}
            //public Dictionary<DateTime, List<Models.Task>> DistributeTasks(List<Models.Task> tasks)
            //{
            //    var schedule = new Dictionary<DateTime, List<Models.Task>>();
            //    double maxDailyHours = 5; // Лимит часов в день

            //    // Сортировка: сначала высокоприоритетные и ближайшие дедлайны
            //    var sortedTasks = tasks.OrderBy(t => t.Priority)
            //                          .ThenBy(t => t.HardDeadline);

            //    foreach (var task in sortedTasks)
            //    {
            //        TaskInfo currentTaskInfo = new TaskInfo();
            //        currentTaskInfo.Deadline = task.HardDeadline;
            //        currentTaskInfo.Priority = (int)(task.Priority != null ? task.Priority : 2);
            //        currentTaskInfo.Complexity = (int)(task.Complexity != null ? task.Complexity : 5);
            //        currentTaskInfo.AvgCompletionTime =

            //        var startDate = CalculateStartDate(task);
            //        var hoursNeeded = task.AvgCompletionTime.TotalHours;

            //        // Ищем день с достаточным количеством свободного времени
            //        for (var day = startDate; day < task.Deadline; day = day.AddDays(1))
            //        {
            //            if (!schedule.ContainsKey(day.Date))
            //                schedule[day.Date] = new List<Models.Task>();

            //            var busyHours = schedule[day.Date].Sum(t => t.AvgCompletionTime.TotalHours);
            //            if (busyHours + hoursNeeded <= maxDailyHours)
            //            {
            //                schedule[day.Date].Add(task);
            //                break;
            //            }
            //        }
            //    }

            //    return schedule;
            //}


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
