using Microsoft.ML.Data;

namespace Order.Models.Forecast
{
    public class TaskHistoryRecord
    {
        public DateTime Date { get; set; }
        public float TasksCompleted { get; set; }
    }
}
