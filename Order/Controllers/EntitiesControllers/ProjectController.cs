using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Order.Models;
using Order.Models.DTO;
using System.Threading.Tasks;

namespace Order.Controllers.EntitiesControllers
{
    [ApiController]
    //[Authorize]
    [Route("api/[controller]")]
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProjectController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

        // GET: api/Project/{userId}
        // Получить все проекты + их задачи у пользователя
        [HttpGet("{userId:Guid}")]
        public async Task<IActionResult> GetProjectsByUserId(Guid userId)
        {
            var projects = await _context.Projects
                    .Where(project => project.UserId == userId)
                    .Include(project => project.Tasks)
                    .ToListAsync();

            if (!projects.Any())
            {
                return NotFound();
            }

            return Ok(projects);
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
        public async Task<IActionResult> UpdateProject(int id, [FromBody] ProjectDto updatedProject, [FromServices] TaskService taskService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
               
            else
            {
                // Чтобы не нарушать связь, если userId не изменяется, просто берем то значение, которое уже есть

                if (updatedProject.UserId == null)
                {
                    updatedProject.UserId = project.UserId;
                }

                if (updatedProject.TaskIds == null)
                {
                    //Console.WriteLine("Взяли старые задачи: ", project.TaskIds);
                    Console.WriteLine("TaskIds проекта: " + (project.TaskIds != null ? string.Join(", ", project.TaskIds) : "Нет задач"));
                    updatedProject.TaskIds = project.TaskIds;
                }
                else {
                    Console.WriteLine("Присваиваем новые значения задач");
                }
                //}
                //// Если переданы новые задачи, привязываем их
                //else
                //{
                //    Console.WriteLine("Были получены новые задачи, перепривязываем...");
                //    var tasksToUpdate = await _context.Tasks
                //        .Where(t => updatedProject.TaskIds.Contains(t.Id))
                //        .ToListAsync();

                //    // Если количество найденных задач не совпадает с количеством переданных id
                //    if (tasksToUpdate.Count != updatedProject.TaskIds.Count)
                //    {
                //        return BadRequest("Некоторые из переданных задач не найдены. Изменения не были применены.");
                //    }
                //    else
                //    {
                //        await taskService.UnassignTasksFromProject(id);
                //        foreach (var task in tasksToUpdate)
                //        {
                //            await taskService.AssignTasksToProject(id, updatedProject.TaskIds);
                //        }

                //    }
                //}

                // Обновление полей объекта маппингом
                _mapper.Map(updatedProject, project);

                // Сохраняем изменения
                _context.Projects.Update(project);
                await _context.SaveChangesAsync();

                return NoContent();
            }

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
