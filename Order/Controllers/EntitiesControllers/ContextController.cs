using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order;
using Order.Models;
using Order.Models.DTO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Order.Controllers.EntitiesControllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ContextController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ContextController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Context/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetContextById(int id)
        {
            var context = await _context.Contexts.FindAsync(id);
            if (context == null)
                return NotFound();
            return Ok(context);
        }

        
        // POST: api/Context
        [HttpPost]
        public async Task<IActionResult> CreateContext([FromBody] Context newContext)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Contexts.Add(newContext);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetContextById), new { id = newContext.Id }, newContext);
        }

        // PUT: api/Context/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateContext(int id, [FromBody] JsonElement body, [FromServices] MainService mainService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var context = await _context.Contexts.FindAsync(id);
            if (context == null)
                return NotFound();

            // Преобразуем JSON в DTO
            var updatedContext = JsonSerializer.Deserialize<ContextDto>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (updatedContext == null)
                return BadRequest("Invalid JSON format.");

            var jsonString = body.ToString();
            var jsonDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

            try
            {
                // Устанавливаем значения null для соответствующих полей
                mainService.SetNullFields(context, jsonDict);

                _mapper.Map(updatedContext, context);

                // Валидация
                mainService.ValidateEntityForNotNullConstraints(context);

                _context.Contexts.Update(context);
                await _context.SaveChangesAsync();
            }
            catch (InvalidOperationException ex)
            {
                // Возвращаем ошибку, если поле не допускает null
                return BadRequest(new { error = ex.Message });
            }
            catch (DbUpdateException dbEx)
            {
                // Перехватываем исключение уровня базы данных
                return BadRequest(new { error = "Ошибка базы данных", details = dbEx.Message });
            }

            return NoContent();
        }

        
        // DELETE: api/Context/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteContext(int id)
        {
            var context = await _context.Contexts.FindAsync(id);
            if (context == null)
                return NotFound();
            try
            {
                _context.Contexts.Remove(context);
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
