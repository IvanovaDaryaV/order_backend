using Microsoft.ML.Data;

namespace Order.Models
{
    // Результат прогноза
    public class TaskForecast
    {
        public float[] ForecastedTasks { get; set; }
    }
}
