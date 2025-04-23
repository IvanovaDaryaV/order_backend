using HtmlAgilityPack;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
//using PorterStemmer;

namespace Order.Services
{
    public class EventParser
    {
        private readonly HttpClient _httpClient;

        public EventParser(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<string>> GetEventsAsync(string url)
        {
            // получение HTML страницы
            var response = await _httpClient.GetStringAsync(url);

            // загрузка HTML в HtmlAgilityPack
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var events = new List<string>();

            // XPath для поиска блоков с событиями
            var eventNodes = htmlDoc.DocumentNode.SelectNodes("//article[contains(@class, 'article')]");
            if (eventNodes != null)
            {
                foreach (var eventNode in eventNodes)
                {
                    var dateNode = eventNode.SelectSingleNode(".//div[@class='date']");
                    string date = null;

                    if (dateNode != null)
                    {
                        var day = dateNode.SelectSingleNode(".//div[@class='day']")?.InnerText.Trim();
                        var month = dateNode.SelectSingleNode(".//div[@class='month']")?.InnerText.Trim();
                        var year = dateNode.SelectSingleNode(".//div[@class='year']")?.InnerText.Trim();
                        date = $"{day} {month} {year}";
                    }

                    var titleNode = eventNode.SelectSingleNode(".//div[@class='article_title']/a");
                    var title = titleNode?.InnerText.Trim();

                    // формирование строку с результатом
                    if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(title))
                    {
                        events.Add($"{date}: {title}");
                    }
                }
            }
            else
            {
                Console.WriteLine("События не найдены на странице.");
            }
            //await ClusterEvents(events);
            return events;
        }
        //public class ClusterPrediction
        //{
        //    [ColumnName("PredictedLabel")]
        //    public uint ClusterId { get; set; }

        //    public float[] Score { get; set; }
        //}
        //public class NewsData
        //{
        //    public string Date { get; set; }
        //    public string Text { get; set; }
        //    public string Category { get; set; }
        //}
        // Метод для кластеризации новостей с сайта по темам
        //public async Task<List<string>> ClusterEvents(List<string> events)
        //{
        //    // Преобразование списка строк в список объектов NewsData
        //    var news = events.Select(e =>
        //    {
        //        var parts = e.Split(':');
        //        return new NewsData
        //        {
        //            Date = parts[0].Trim(), // дата
        //            Text = parts.Length > 1 ? parts[1].Trim() : string.Empty // текст события
        //        };
        //    }).ToList();

        //    // Создание контекста ML.NET
        //    var mlContext = new MLContext();

        //    var data = mlContext.Data.LoadFromEnumerable(news);

        //    // Преобразование текста в TF-IDF
        //    var dataPipeline = mlContext.Transforms.Text.FeaturizeText("Features", nameof(NewsData.Text))
        //    .Append(mlContext.Clustering.Trainers.KMeans("Features", numberOfClusters: 7));

        //    // Обучение модели
        //    var model = dataPipeline.Fit(data);

        //    // 6.Прогнозирование
        //    var predictions = model.Transform(data);
        //    var clusteredResults = mlContext.Data.CreateEnumerable<ClusterPrediction>(predictions, reuseRowObject: false).ToList();

        //    // 7. Вывод результатов
        //    Console.WriteLine("Кластеризация новостей:");
        //    for (int i = 0; i < news.Count; i++)
        //    {
        //        Console.WriteLine($"Новость: {news[i].Text}");
        //        Console.WriteLine($"Кластер: {clusteredResults[i].ClusterId}");
        //        Console.WriteLine();
        //    }

        //    return events;
        //}
    }
}
