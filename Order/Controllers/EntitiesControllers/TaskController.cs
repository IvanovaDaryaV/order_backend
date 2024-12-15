using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.Models;
using Order.Models.DTO;

namespace Order.Controllers.EntitiesControllers
{
    [ApiController]
    //[Authorize]
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
        public async Task<IActionResult> GetTaskById(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();
            return Ok(task);
        }

        [HttpGet("inbox/{userId:Guid}")]
        public async Task<IActionResult> GetInboxTasks(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var inboxTasks = await _context.Tasks
                .Where(task => task.UserId == user.Id &&
                task.ContextId == null && task.ProjectId == null && task.EventId == null && task.CallendarDate == null)
                .ToListAsync();

            return Ok(inboxTasks);
        }

        // POST: api/Task
        [HttpPost]
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
        public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskDto updatedTask)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            // Чтобы не нарушать связь, если userId не изменяется, просто берем то значение,
            // которое сейчас в задаче

            if (updatedTask.UserId == null)
            {
                updatedTask.UserId = task.UserId;
            }


            _mapper.Map(updatedTask, task);

            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Task/{id}
        [HttpDelete("{id:int}")]
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
