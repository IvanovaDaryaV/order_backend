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

        // Получение новостей с официального сайта https://www.utmn.ru/news/events/
        public async Task<List<string>> GetEventsTMNofficial(string url)
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

        // Получение новостей ИГИП https://www.utmn.ru/igip/
        public async Task<List<string>> GetEventsTMNigip(string url)
        {
            var events = new List<string>();

            for (int page = 1; page <= 5; page++)
            {
                url = $"https://www.utmn.ru/igip/?PAGEN_1={page}";
                var response = await _httpClient.GetAsync(url);
                var pageHtml = await response.Content.ReadAsStringAsync();
                var htmlDoc = new HtmlDocument();


                events = new List<string>();

                // XPath для поиска блоков с событиями
                var eventNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'news-item')]");
                if (eventNodes != null)
                {
                    foreach (var eventNode in eventNodes)
                    {
                        var textNode = eventNode.SelectSingleNode(".//div[contains(@class, 'news-item_text')]");
                        if (textNode == null) continue;

                        var dateNode = textNode.SelectSingleNode(".//span[contains(@class, 'news-item__date')]");
                        var titleNode = textNode.SelectSingleNode(".//h4");

                        var date = dateNode?.InnerText.Trim();
                        var title = titleNode?.InnerText.Trim();

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
                foreach (var ev in events)
                {
                    Console.WriteLine(ev);
                }
            }
            return events;
        }
    }
}
