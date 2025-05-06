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
        // Получить все проекты + их ЗАДАЧИ, ЗАМЕТКИ и СОБЫТИЯ у пользователя

        // Для отрисовки вкладки с проектами
        [HttpGet("{userId:Guid}")]
        public async Task<IActionResult> GetProjectByUserId(Guid userId)
        {
            var projects = await _context.Projects
                    //.Where(up => up.UserId == userId)
                    //.Select(up => up.Project)
                    .Where(p => p.ProjectUsers.Any(pu => pu.UserId == userId))
                    .Include(project => project.Tasks)
                    .Include(project => project.Events)
                    .Include(project => project.Notes)
                    .ToListAsync();

            if (!projects.Any())
            {
                return NotFound("Для данного userId не найдены привязанные к нему проекты");
            }

            return Ok(projects);
        }

        // POST: api/Project
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] Project newProject, Guid userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            newProject.ProjectUsers = new List<ProjectUser>
            {
                new ProjectUser
                {
                    UserId = userId,
                    ProjectId = newProject.Id,
                }
            };

            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjectById), new { id = newProject.Id }, newProject);
        }

        [HttpPost("{projectId:int}/users")]
        public async Task<IActionResult> AddUsersToProject(int projectId, [FromBody] Guid[] userIds)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            foreach (var userId in userIds)
            {
                var exists = await _context.ProjectUser
                    .AnyAsync(p => p.ProjectId == projectId && p.UserId == userId);

                if (!exists)
                {
                    _context.ProjectUser.Add(new ProjectUser
                    {
                        ProjectId = projectId,
                        UserId = userId
                    });
                }
            }
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{projectId:int}/users")]
        public async Task<IActionResult> RemoveUsersFromProject(int projectId, [FromBody] Guid[] userIds)
        {

            var recordsToDelete = await _context.ProjectUser
                .Where(p => p.ProjectId == projectId && userIds.Contains(p.UserId))
                .ToListAsync();

            if (!recordsToDelete.Any())
                return NotFound("Для указанного проекта не найдено пользователей");

            _context.ProjectUser.RemoveRange(recordsToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Project/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateProject(int id, [FromBody] JsonElement body, [FromServices] MainService mainService)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var project = await _context.Projects.Include(p => p.Tasks).Include(p => p.Notes).FirstOrDefaultAsync(p => p.Id == id);
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


                // Проверка: было ли передано новое значение TaskIds ----------------------------
                if (jsonDict.ContainsKey("taskIds"))
                {
                    Console.WriteLine("ПОЛУЧЕН СПИСОК ЗАДАЧ");
                    // Значение было передано и оно не null
                    if (jsonDict["taskIds"] != null)
                    {
                        Console.WriteLine("СПИСОК ЗАДАЧ НЕПУСТОЙ");
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
                            // Конфликт возникает, если у задачи из списка уже есть проект, к которому она привязана,
                            // и этот проект не текущий, а другой.
                            // Если задача уже привязана к этому проекту, конфликта не будет
                            // Например, чтобы дополнить список, не нужно переприсваивать значения заново
                            bool conflict = false;
                            foreach (var task in tasksToUpdate)
                            {
                                if (task.ProjectId != null && task.ProjectId != id)
                                    conflict = true;
                            }

                            if (!conflict)
                            {
                                Console.WriteLine("КОНФЛИКТА НЕТ, ПЕРЕПРИВЯЗЫВАЕМ ЗАДАЧИ");
                                await mainService.UnassignTasksFromProject(id);
                                await mainService.AssignTasksToProject(id, updatedProject.TaskIds);

                                // Обновим связанные задачи вручную, чтобы они не были перезаписаны
                                project.Tasks = await _context.Tasks.Where(t => t.ProjectId == id).ToListAsync();
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

                // Проверка: было ли передано новое значение NoteIds ----------------------------
                if (jsonDict.ContainsKey("noteIds"))
                {
                    // Значение было передано и оно не null
                    if (jsonDict["noteIds"] != null)
                    {
                        var notesToUpdate = await _context.Notes
                            .Where(t => updatedProject.NoteIds.Contains(t.Id))
                            .ToListAsync();

                        // Если количество найденных заметок не совпадает с количеством переданных id
                        if (notesToUpdate.Count != updatedProject.NoteIds.Count)
                        {
                            return BadRequest("Некоторые из переданных заметок не найдены.  Изменения не были применены.");
                        }
                        else
                        {
                            // Конфликт возникает, если у заметки из списка уже есть проект, к которому она привязана,
                            // и этот проект не текущий, а другой.
                            // Если заметка уже привязана к этому проекту, конфликта не будет
                            // Например, чтобы дополнить список, не нужно переприсваивать значения заново
                            bool conflict = false;
                            foreach (var note in notesToUpdate)
                            {
                                if (note.ProjectId != null && note.ProjectId != id)
                                    conflict = true;
                            }

                            if (!conflict)
                            {
                                await mainService.UnassignNotesFromProject(id);
                                await mainService.AssignNotesToProject(id, updatedProject.NoteIds);
                            }
                            else
                            {
                                return BadRequest("Конфликт: заметка(и) уже привязана к другому проекту.  Изменения не были применены.");
                            }
                        }
                    }
                    // Если передано значение null
                    else
                    {
                        await mainService.UnassignNotesFromProject(id);
                    }
                }
                // Если новых заметок не было - без изменений
                else
                {
                    var notesIds = project.Notes.Select(t => t.Id).ToList();
                    updatedProject.NoteIds = notesIds;
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
