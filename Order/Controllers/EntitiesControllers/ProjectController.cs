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
        // Получить все проекты + их ЗАДАЧИ и СОБЫТИЯ у пользователя

        // Для отрисовки вкладки с проектами
        [HttpGet("{userId:Guid}")]
        public async Task<IActionResult> GetProjectByUserId(Guid userId)
        {
            var projects = await _context.Projects
                    .Where(project => project.UserId == userId)
                    .Include(project => project.Tasks)
                    .Include(project => project.Events)
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
        public async Task<IActionResult> UpdateProject(int id, [FromBody] JsonElement body, [FromServices] MainService mainService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = await _context.Projects.Include(p => p.Tasks).FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            try
            {
                // Преобразуем JSON в DTO
                var updatedProject = JsonSerializer.Deserialize<ProjectDto>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (updatedProject == null)
                    return BadRequest("Invalid JSON format.");

                var jsonString = body.ToString();
                var jsonDict = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);


                // Чтобы не нарушать связь, если userId не изменяется, просто берем то значение, которое уже есть

                if (updatedProject.UserId == null)
                {
                    updatedProject.UserId = project.UserId;
                }


                // Проверка: было ли передано новое значение TaskIds
                if (jsonDict.ContainsKey("taskIds"))
                {
                    // Значение было передано и оно не null
                    if (jsonDict["taskIds"] != null)
                    {
                        var tasksToUpdate = await _context.Tasks
                            .Where(t => updatedProject.TaskIds.Contains(t.Id))
                            .ToListAsync();

                        // Если количество найденных задач не совпадает с количеством переданных id
                        if (tasksToUpdate.Count != updatedProject.TaskIds.Count)
                        {
                            return BadRequest("Некоторые из переданных задач не найдены.  Изменения не были применены.");
                        }
                        else
                        {
                            bool conflict = false;
                            foreach (var task in tasksToUpdate)
                            {
                                if (task.ProjectId != null)
                                    conflict = true;
                            }

                            if (!conflict)
                            {
                                await mainService.UnassignTasksFromProject(id);
                                foreach (var task in tasksToUpdate)
                                {
                                    await mainService.AssignTasksToProject(id, updatedProject.TaskIds);
                                }
                            }
                            else
                            {
                                return BadRequest("Конфликт: задача(и) уже привязана к другому проекту.  Изменения не были применены.");
                            }    
                        }
                    }
                    // Если передано значение null
                    else
                    {
                        await mainService.UnassignTasksFromProject(id);
                    }
                }
                // Если новых задач не было - без изменений
                else
                {
                    var taskIds = project.Tasks.Select(t => t.Id).ToList();
                    updatedProject.TaskIds = taskIds;
                }

                // Устанавливаем значения null для соответствующих полей
                mainService.SetNullFields(project, jsonDict);

                // Обновление полей объекта маппингом
                _mapper.Map(updatedProject, project);

                // Валидация
                mainService.ValidateEntityForNotNullConstraints(project);

                // Сохраняем изменения
                _context.Projects.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
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
