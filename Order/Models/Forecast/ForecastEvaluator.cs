/*
 Оценка качества прогноза
 */

namespace Order.Models.Forecast
{
    public class ForecastEvaluator
    {
        //public EvaluationMetrics Evaluate(float[] actual, float[] predicted)
        //{
        //    return new EvaluationMetrics
        //    {
        //        MAE = CalculateMAE(actual, predicted),
        //        RMSE = CalculateRMSE(actual, predicted),
        //        R2 = CalculateR2(actual, predicted)
        //    };
        //}

        private float CalculateMAE(float[] actual, float[] predicted)
        {
            return actual.Zip(predicted, (a, p) => Math.Abs(a - p)).Average();
        }

        // ... другие метрики
    }
}
