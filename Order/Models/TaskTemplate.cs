namespace Order.Models
{
    public class TaskTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } // Название шаблона, например, "Лекция"
        public string Description { get; set; } // Описание задачи по умолчанию
        public int? DefaultPriority { get; set; } // Приоритет по умолчанию
        public TimeSpan? DefaultDuration { get; set; } // Продолжительность по умолчанию
        public bool IsRecurring { get; set; } // Повторяющаяся задача
    }

}
