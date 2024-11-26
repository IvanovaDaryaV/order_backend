using Microsoft.AspNetCore.Mvc;
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
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();
            return Ok(project);
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
        public async Task<IActionResult> UpdateProject(int id, [FromBody] Project updatedProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound();

            project.Description = updatedProject.Description;
            project.HardDeadline = updatedProject.HardDeadline;
            project.SoftDeadline = updatedProject.SoftDeadline;
            project.Status = updatedProject.Status;
            project.ContextId = updatedProject.ContextId;
            project.Priority = updatedProject.Priority;
            project.UserId = updatedProject.UserId;
            project.User = updatedProject.User;
            project.Context = updatedProject.Context;

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
