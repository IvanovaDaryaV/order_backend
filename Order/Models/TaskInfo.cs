using Microsoft.ML.Data;

/*
 Класс для предсказаний
 */
namespace Order.Models
{
    public class TaskInfo
    {
        public DateTime Deadline { get; set; }
        public int Priority { get; set; }       // 1-5
        public int Complexity { get; set; }  // 1-10
        public TimeSpan AvgCompletionTime { get; set; } // Среднее время на подобные задачи
    }

}
