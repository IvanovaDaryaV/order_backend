using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Models;
using Order.Models.DTO;

namespace Order.Controllers.EntitiesControllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TaskController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Task/{id}
        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();
            return Ok(task);
        }

        // POST: api/Task
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateTask([FromBody] Models.Task newTask)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTaskById), new { id = newTask.Id }, newTask);
        }

        // PUT: api/Task/{id}
        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskDto updatedTask)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            _mapper.Map(updatedTask, task);

            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Task/{id}
        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<IActionResult> DeleteTask(int id, [FromServices] TaskService taskService)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();
            try
            {
                await taskService.RemoveTask(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
