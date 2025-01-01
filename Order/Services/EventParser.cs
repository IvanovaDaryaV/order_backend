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
            var eventNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'article')]");

            if (eventNodes != null)
            {
                foreach (var eventNode in eventNodes)
                {
                    // Извлекаем дату события
                    var dateNode = eventNode.SelectSingleNode(".//div[contains(@class, 'article_date')]");
                    var date = dateNode?.InnerText.Trim();

                    // Извлекаем название события
                    var infoNode = eventNode.SelectSingleNode(".//div[contains(@class, 'article_info')]");
                    if (infoNode != null)
                    {
                        // Извлекаем название события из article_title
                        var titleNode = infoNode.SelectSingleNode(".//div[contains(@class, 'article_title')]/a");
                        var title = titleNode?.InnerText.Trim();

                        // Формируем строку с результатом
                        if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(title))
                        {
                            events.Add($"{date}: {title}");
                        }
                    }
                }
            }

            return events;
        }
    }
}
