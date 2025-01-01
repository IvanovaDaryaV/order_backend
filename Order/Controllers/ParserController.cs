using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Order.Services;
using System.ComponentModel;
using System.Threading.Tasks;
using Order.Models;

namespace Order.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class ParserController : Controller
    {
        private readonly EventParser _eventParser;
        private readonly ApplicationDbContext _context;

        public ParserController(EventParser eventParser, ApplicationDbContext context)
        {
            _eventParser = eventParser;
            _context = context;
        }

        // получение новостей с сайта, результат - список строк дата+название события
        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var url = "https://www.utmn.ru/news/events/";
            var events = await _eventParser.GetEventsAsync(url);

            //foreach (var ev in events)
            //{
            //    Console.WriteLine(ev);
            //}

            return Ok(events);
        }

        [HttpPost("add-events-to-calender")]
        public async Task<IActionResult> AddEventsFromICS(Guid userId, string eventText)
        {
            string[] words = eventText.Split(':');

            string date = words[0].Trim();
            string[] tmp = date.Split(' ');
            int month;
            TryParseMonth(tmp[1], out month);
            

            string newDate = $"{tmp[0]}/{month}/{tmp[2]} 00:00:00";


            Event evt = new Event();
            evt.Name = words[1].Trim();
            evt.PeriodStart = DateTime.Parse(newDate);
            evt.PeriodEnd = DateTime.Parse(newDate);
            evt.UserId = userId;
            evt.Type = "uni event";

            if (evt.PeriodEnd > DateTime.Today) evt.Status = false;
            else evt.Status = true;

            _context.Events.Add(evt);

            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool TryParseMonth(string monthText, out int month)
        {
            var months = new Dictionary<string, int>
            {
                { "янв", 1 }, { "фев", 2 }, { "мар", 3 }, { "апр", 4 },
                { "май", 5 }, { "июн", 6 }, { "июл", 7 }, { "авг", 8 },
                { "сен", 9 }, { "окт", 10 }, { "ноя", 11 }, { "дек", 12 }
            };

            return months.TryGetValue(monthText?.ToLower(), out month);
        }
    }
}
