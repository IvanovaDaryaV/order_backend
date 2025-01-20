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
        private readonly IMapper _mapper;

        public NoteController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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
            newNote.LastEdited = DateTime.Now;

            _context.Notes.Add(newNote);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetNoteById), new { id = newNote.Id }, newNote);
        }

        // PUT: api/Note/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateNote(int id, [FromBody] JsonElement body, [FromServices] MainService mainService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
                return NotFound();

            try
            {
                // Преобразуем JSON в DTO
                var updatedNote = JsonSerializer.Deserialize<NoteDto>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (updatedNote == null)
                    return BadRequest("Invalid JSON format.");

                var jsonString = body.ToString();
                var jsonDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

                // Устанавливаем значения null для соответствующих полей
                mainService.SetNullFields(note, jsonDict);

                // Чтобы не нарушать связь, если userId не изменяется, просто берем то значение, которое уже есть

                if (updatedNote.UserId == null)
                {
                    updatedNote.UserId = note.UserId;
                }

                updatedNote.LastEdited = DateTime.Now;

                _mapper.Map(updatedNote, note);

                // Валидация
                mainService.ValidateEntityForNotNullConstraints(note);

                _context.Notes.Update(note);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
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
