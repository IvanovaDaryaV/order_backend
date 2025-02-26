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
    //[Authorize]
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
                .Where(note => note.Tag == "Inbox")
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

            // Заметка не может иметь привязку к проекту, если она привязана к какому-то из разделов
            if (newNote.Tag != null && newNote.ProjectId != null)
            {
                return BadRequest("Конфликт: заметка не может одновременно принадлежать к разделу и быть привязана к проекту. Заметка не была создана.");
            }

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

                // Чтобы не нарушать связь, если userId не изменяется, просто берем то значение, которое уже есть

                if (updatedNote.UserId == null)
                {
                    updatedNote.UserId = note.UserId;
                }
                if (updatedNote.ProjectId == null)
                {
                    updatedNote.ProjectId = note.ProjectId;
                }

                // Устанавливаем значения null для соответствующих полей
                mainService.SetNullFields(note, jsonDict);

                
                updatedNote.LastEdited = DateTime.Now;

                // Если при изменении заметки было передано значение для projectId
                // не рассматривается ситуация, когда поле projectId = null, 
                // потому что отвязать заметку от проекта нельзя, можно только удалить на совсем
                if (jsonDict.ContainsKey("projectId"))
                {
                    if (jsonDict["projectId"] != null)
                    {
                        // обновление списка заметок, которые принадлежат к указанному проекту
                        var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == updatedNote.ProjectId);
                        if (project == null)
                        {
                            return BadRequest("Проект не существует");
                        }
                        else
                        {
                            // Очистка поля tag
                            if (jsonDict.ContainsKey("tag") && jsonDict["tag"] != null)
                            {
                                note.Tag = null;
                            }

                            project.NoteIds.Add(note.Id);
                        }
                    }
                }

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
