using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.Models;

namespace Order.Controllers.EntitiesControllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProjectController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Project/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProjectById(int id)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
            var tasks = _context.Tasks
                        .Where(task => project.TaskIds.Contains(task.Id))
                        .ToList();
            if (project == null)
                return NotFound();
            return Ok(new {
                project,
                tasks
            });
        }

        // POST: api/Project
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] Project newProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProjectById), new { id = newProject.Id }, newProject);
        }

        // PUT: api/Project/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] Project updatedProject, [FromServices] TaskService taskService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();


            // Если переданы новые задачи, привязываем их
            if (updatedProject.TaskIds != null && updatedProject.TaskIds.Any())
            { 
                var tasksToUpdate = await _context.Tasks
                    .Where(t => updatedProject.TaskIds.Contains(t.Id))
                    .ToListAsync();

                // Если количество найденных задач не совпадает с количеством переданных id
                if (tasksToUpdate.Count != updatedProject.TaskIds.Count)
                {
                    return BadRequest("Некоторые из переданных задач не найдены. Изменения не были применены.");
                }
                else
                {
                    await taskService.UnassignTasksFromProject(id);
                    foreach (var task in tasksToUpdate)
                    {
                        await taskService.AssignTasksToProject(id, updatedProject.TaskIds);
                    }
                }
            }

            // Обновляем остальные поля проекта
            project.Description = updatedProject.Description;
            project.HardDeadline = updatedProject.HardDeadline;
            project.SoftDeadline = updatedProject.SoftDeadline;
            project.Status = updatedProject.Status;
            project.ContextId = updatedProject.ContextId;
            project.Priority = updatedProject.Priority;
            project.UserId = updatedProject.UserId;
            project.User = updatedProject.User;
            project.Context = updatedProject.Context;
            project.TaskIds = updatedProject.TaskIds;

            // Сохраняем изменения
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        // DELETE: api/Project/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
