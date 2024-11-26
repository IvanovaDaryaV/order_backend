using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order;
using Order.Models;
using System.Text.Json;

namespace Order.Controllers.EntitiesControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Event/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt == null)
                return NotFound();
            return Ok(evt);
        }

        // POST: api/Event
        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] Event newEvent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEventById), new { id = newEvent.Id }, newEvent);
        }

        // PUT: api/Event/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] Event updatedEvent)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var evt = await _context.Events.FindAsync(id);
            if (evt == null)
                return NotFound();

            evt.Name = updatedEvent.Name;
            evt.Status = updatedEvent.Status;
            evt.ContextId = updatedEvent.ContextId;
            evt.Priority = updatedEvent.Priority;
            evt.CalendarDate = updatedEvent.CalendarDate;
            evt.IsPrivate = updatedEvent.IsPrivate;
            evt.UserId = updatedEvent.UserId;
            evt.User = updatedEvent.User;
            evt.Context = updatedEvent.Context;

            _context.Events.Update(evt);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Event/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var evt = await _context.Events.FindAsync(id);
            if (evt == null)
                return NotFound();

            _context.Events.Remove(evt);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
