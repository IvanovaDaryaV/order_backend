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
            var project = await _context.Projects
                .Include(p => p.Tasks) 
                .Include(p => p.Notes)
                .Include(p => p.Events)
                .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (project == null)
                return NotFound();

            return Ok(project);
        }

        // GET: api/Project/{userId}
        // Получить все проекты + их ЗАДАЧИ, ЗАМЕТКИ и СОБЫТИЯ у пользователя

        // Для отрисовки вкладки с проектами
        [HttpGet("{userId:Guid}")]
        public async Task<IActionResult> GetProjectByUserId(Guid userId)
        {
            var projects = await _context.Projects
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

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return BadRequest("User not found");

            
            newProject.ProjectUsers = new List<ProjectUser>
            {
                new ProjectUser
                {
                    User = user,
                    Project = newProject 
                }
            };

            _context.Projects.Add(newProject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjectById), new { id = newProject.ProjectId }, newProject);
        }

        // Метод добавления пользователей в проект (добавление привязки)
        [HttpPost("{projectId:int}/users")]
        public async Task<IActionResult> AddUsersToProject(int projectId, [FromBody] Guid[] userIds)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectId == projectId);

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

        // Метод удаления пользователей из проекта (удаление привязки)
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

            var project = await _context.Projects.Include(p => p.Tasks).Include(p => p.Notes).FirstOrDefaultAsync(p => p.ProjectId == id);
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

        // Метод привязки задач к проекту (чтобы не передавать полностью объекты задач - только список id)
        [HttpPost("{projectId}/assign-tasks")]
        public async Task<IActionResult> AssignTasksToProject(int projectId, [FromBody] List<int> taskIds)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return NotFound("Проект не найден");

            var tasks = await _context.Tasks
                .Where(t => taskIds.Contains(t.TaskId))
                .ToListAsync();

            // Проверка на конфликты
            var conflictTasks = tasks.Where(t => t.ProjectId != null && t.ProjectId != projectId).ToList();
            if (conflictTasks.Any())
            {
                return BadRequest($"Некоторые задачи уже привязаны к другим проектам: {string.Join(", ", conflictTasks.Select(t => t.TaskId))}");
            }

            // Привязка задач
            foreach (var task in tasks)
            {
                task.ProjectId = projectId;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // Метод отвязки задач от проекта (чтобы не передавать полностью объекты задач - только список id)
        [HttpPost("{projectId}/unassign-tasks")]
        public async Task<IActionResult> UnassignTasksFromProject(int projectId, [FromBody] List<int> taskIds)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return NotFound("Проект не найден");

            var tasks = await _context.Tasks
                .Where(t => taskIds.Contains(t.TaskId) && t.ProjectId == projectId)
                .ToListAsync();

            if (tasks.Count == 0)
                return BadRequest("Не найдены задачи, привязанные к указанному проекту");

            foreach (var task in tasks)
            {
                task.ProjectId = null;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // Метод привязки заметок к проекту
        [HttpPost("{projectId}/assign-notes")]
        public async Task<IActionResult> AssignNotesToProject(int projectId, [FromBody] List<int> noteIds)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return NotFound("Проект не найден");

            var notes = await _context.Notes
                .Where(n => noteIds.Contains(n.NoteId))
                .ToListAsync();

            var conflictNotes = notes.Where(n => n.ProjectId != null && n.ProjectId != projectId).ToList();
            if (conflictNotes.Any())
            {
                return BadRequest($"Некоторые заметки уже привязаны к другим проектам: {string.Join(", ", conflictNotes.Select(n => n.NoteId))}");
            }

            foreach (var note in notes)
            {
                note.ProjectId = projectId;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        // Метод отвязки заметок от проекта
        [HttpPost("{projectId}/unassign-notes")]
        public async Task<IActionResult> UnassignNotesFromProject(int projectId, [FromBody] List<int> noteIds)
        {
            var notes = await _context.Notes
                .Where(n => noteIds.Contains(n.NoteId) && n.ProjectId == projectId)
                .ToListAsync();

            if (!notes.Any())
                return BadRequest("Не найдены заметки, привязанные к проекту");

            foreach (var note in notes)
            {
                note.ProjectId = null;
            }

            await _context.SaveChangesAsync();
            return Ok();
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
