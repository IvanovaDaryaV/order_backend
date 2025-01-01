using HtmlAgilityPack;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Generic;

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
            // Получаем HTML страницы
            var response = await _httpClient.GetStringAsync(url);

            // Загружаем HTML в HtmlAgilityPack
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            // Список для хранения событий
            var events = new List<string>();

            // XPath для поиска блоков с событиями
            var eventNodes = htmlDoc.DocumentNode.SelectNodes("//article[contains(@class, 'article')]");
            if (eventNodes != null)
            {
                foreach (var eventNode in eventNodes)
                {
                    // Извлекаем дату события
                    var dateNode = eventNode.SelectSingleNode(".//div[@class='date']");
                    string date = null;

                    if (dateNode != null)
                    {
                        // Собираем дату из дочерних узлов
                        var day = dateNode.SelectSingleNode(".//div[@class='day']")?.InnerText.Trim();
                        var month = dateNode.SelectSingleNode(".//div[@class='month']")?.InnerText.Trim();
                        var year = dateNode.SelectSingleNode(".//div[@class='year']")?.InnerText.Trim();
                        date = $"{day} {month} {year}";
                    }

                    // Извлекаем название события
                    var titleNode = eventNode.SelectSingleNode(".//div[@class='article_title']/a");
                    var title = titleNode?.InnerText.Trim();

                    // Формируем строку с результатом
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

            return events;
        }


    }
}
