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

        private static readonly Dictionary<string, string> InstituteNewsUrls = new()
        {
            { "official", "https://www.utmn.ru/news/events/" }, // общие новости
            { "igip", "https://www.utmn.ru/igip/" },            // ИГИП
            { "scs", "https://www.utmn.ru/scs/novosti/" },      // ШКН
            { "ihss", "https://www.utmn.ru/ihss/" },            // СОЦГУМ
            { "ifk", "https://www.utmn.ru/ifk/" },              // институт ФИЗРЫ
            { "riic", "https://www.utmn.ru/riic/events/" },     // рег. институт междунар. сотрудничества
            { "fei", "https://www.utmn.ru/fei/" },              // ФЭИ
            { "sns", "https://www.utmn.ru/sns/" },              // шк естественных наук
            { "soe", "https://www.utmn.ru/soe/" },              // шк образования

        };

        // получение новостей с сайта
        [HttpGet]
        public async Task<IActionResult> GetEventsTMNofficial(string? instName)
        {
            List<string> events = new List<string>();
            if (instName == null)
                instName = String.Empty;

            if (instName == "official")
            {
                events = await _eventParser.GetEventsTMNofficial(InstituteNewsUrls["official"]);
            }
            else if (InstituteNewsUrls.ContainsKey(instName))
            {
                events = await _eventParser.GetEventsTMN(InstituteNewsUrls[instName]);
            }
            else
            {
                List<string> result = new List<string>();
                foreach (string url in InstituteNewsUrls.Keys)
                {
                    events = await _eventParser.GetEventsTMN(InstituteNewsUrls[url]);
                    foreach (string ev in events)
                    {
                        result.Add(ev);
                    }
                }
                return Ok(result);
            }
            return Ok(events);
        }

        [HttpPost("add-events-to-calender")]
        public async Task<IActionResult> AddEventsFromSite(Guid userId, string eventText)
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
