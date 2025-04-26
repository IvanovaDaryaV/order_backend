using HtmlAgilityPack;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.ML;
using Microsoft.ML.Data;
using System.Collections;
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
            Console.WriteLine(events);
            return events;
        }
    }
}
