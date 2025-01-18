using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.Models;
using Order.Models.DTO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Order.Controllers.EntitiesControllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class NoteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NoteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/inbox/{id}
        [HttpGet("inbox/{userId:Guid}")]
        public async Task<IActionResult> GetInbox(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var inboxNotes = await _context.Notes
                .Where(note => note.UserId == user.Id)
                .ToListAsync();

            return Ok(inboxNotes);
        }

        // GET: api/Note/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return NotFound();
            return Ok(note);
        }

        // POST: api/Note
        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] Note newNote)
        {
            newNote.DateCreated = DateTime.Now;

            _context.Notes.Add(newNote);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetNoteById), new { id = newNote.Id }, newNote);
        }

        // PUT: api/Task/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateNote(int id, string newText)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return NotFound();

            note.Text = newText;
            _context.Notes.Update(note);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        // DELETE: api/Note/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return NotFound();
            try
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
