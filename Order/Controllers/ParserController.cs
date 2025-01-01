using Microsoft.AspNetCore.Mvc;
using Order.Services;
using System.Threading.Tasks;

namespace Order.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class ParserController : Controller
    {
        private readonly EventParser _eventParser;

        public ParserController(EventParser eventParser)
        {
            _eventParser = eventParser;
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents()
        {
            var url = "https://www.utmn.ru/news/events/";
            var events = await _eventParser.GetEventsAsync(url);
            return Ok(events);
        }
    }
}
