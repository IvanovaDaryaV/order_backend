/*using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.TimeSeries;

using Order.Models.Forecast;
using Order.Models;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

*//*
 Сервис по прогнозированию (анализ временных рядов)
 *//*

namespace Order.Services
{
    public class TaskForecastService
    {
        private readonly ApplicationDbContext _context;
        private readonly MLContext _mlContext;
        private ITransformer _trainedModel;
        private TimeSeriesPredictionEngine<TaskHistoryRecord, TaskForecast> _forecastEngine;

        public TaskForecastService(ApplicationDbContext context)
        {
            _context = context;
            _mlContext = new MLContext();
        }
        public async Task<List<TaskHistoryRecord>> GetTaskHistoryAsync(Guid userId)
        {
            return await _context.Tasks
                .Where(t => t.UserId == userId && t.DateDone != null)
                .GroupBy(t => t.DateDone) // Группировка по дням
                .Select(g => new TaskHistoryRecord
                {
                    Date = (DateTime)g.Key,
                    TasksCompleted = g.Count(), // Количество завершенных задач в день
                    //TotalTaskTime = g.Sum(t => (float)t.TimeSpent.TotalHours) // Опционально: суммарное время
                })
                .OrderBy(r => r.Date)
                .ToListAsync();
        }
        public void TrainModel(IEnumerable<TaskHistoryRecord> historicalData)
        {
            // 1. Подготовка данных
            var dataView = _mlContext.Data.LoadFromEnumerable(historicalData);

            // 2. Настройка конвейера
            var pipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(TaskForecast.ForecastedTasks),
                inputColumnName: nameof(TaskHistoryRecord.TasksCompleted),
                windowSize: 7,       // Анализировать недельные циклы
                seriesLength: 30,     // Использовать данные за 30 дней
                trainSize: 100,       // Общее количество точек данных
                horizon: 7           // Прогноз на 7 дней вперед
            );

            // 3. Обучение модели
            _trainedModel = pipeline.Fit(dataView);
        }

        public float[] Predict(int daysToForecast = 7)
        {
            if (_forecastEngine == null)
                throw new InvalidOperationException("Модель не обучена");

            var forecast = _forecastEngine.Predict();
            return forecast.ForecastedTasks[..daysToForecast];
        }

        private readonly ConcurrentDictionary<int, ITransformer> _userModels = new();

        // Кэширование моделей
        //public void TrainModelForUser(int userId, IEnumerable<TaskHistoryRecord> data)
        //{
        //    var model = TrainModelInternal(data);
        //    _userModels.AddOrUpdate(userId, model, (id, oldModel) => model);
        //}

        //public float[] PredictForUser(int userId)
        //{
        //    if (!_userModels.TryGetValue(userId, out var model))
        //        throw new KeyNotFoundException("Модель для пользователя не найдена");

        //    // ... реализация прогноза с использованием сохраненной модели
        //}

    }
}
*/