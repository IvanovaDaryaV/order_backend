using Microsoft.AspNetCore.Mvc;
using Order.Models;

namespace Order.Controllers.EntitiesControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
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
        public async Task<IActionResult> UpdateTask(int id, [FromBody] Models.Task updatedTask)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
                return NotFound();

            task.Name = updatedTask.Name;
            task.Description = updatedTask.Description;
            task.Priority = updatedTask.Priority;
            task.ContextId = updatedTask.ContextId;
            task.HardDeadline = updatedTask.HardDeadline;
            task.SoftDeadline = updatedTask.SoftDeadline;
            task.UserId = updatedTask.UserId;
            task.Status = updatedTask.Status;
            task.User = updatedTask.User;
            task.Context = updatedTask.Context;

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
