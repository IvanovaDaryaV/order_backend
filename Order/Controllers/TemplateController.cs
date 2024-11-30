/*using Microsoft.AspNetCore.Mvc;
using Order.Models;
using System;

namespace Order.Controllers
{
    [ApiController]
    [Route("api/templates")]
    public class TaskTemplateController : ControllerBase
    {
        private readonly ITaskTemplateRepository _templateRepository;

        public TaskTemplateController(ITaskTemplateRepository templateRepository)
        {
            _templateRepository = templateRepository;
        }

        [HttpGet]
        public IActionResult GetTemplates()
        {
            var templates = _templateRepository.GetAll();
            return Ok(templates);
        }

        [HttpPost("apply")]
        public IActionResult CreateTaskFromTemplate([FromBody] ApplyTemplateRequest request)
        {
            var template = _templateRepository.GetById(request.TemplateId);
            if (template == null)
            {
                return NotFound("Template not found");
            }

            var newTask = new Task
            {
                Name = template.Name,
                Description = template.Description,
                Priority = template.DefaultPriority,
                Duration = template.DefaultDuration,
                IsRecurring = template.IsRecurring,
                Date = request.Date
            };

            // Сохраняем новую задачу в базе данных (или через соответствующий сервис)
            _taskRepository.Add(newTask);

            return Ok(newTask);
        }
    }

    public class ApplyTemplateRequest
    {
        public int TemplateId { get; set; } // ID шаблона
        public DateTime Date { get; set; } // Дата задачи
    }

    public interface ITaskTemplateRepository
    {
        IEnumerable<TaskTemplate> GetAll();
        TaskTemplate GetById(int id);
    }

    public class TaskTemplateRepository : ITaskTemplateRepository
    {
        private readonly AppDbContext _dbContext;

        public TaskTemplateRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IEnumerable<TaskTemplate> GetAll()
        {
            return _dbContext.TaskTemplates.ToList();
        }

        public TaskTemplate GetById(int id)
        {
            return _dbContext.TaskTemplates.FirstOrDefault(t => t.Id == id);
        }
    }

    public static void SeedTemplates(AppDbContext context)
    {
        if (!context.TaskTemplates.Any())
        {
            context.TaskTemplates.AddRange(new List<TaskTemplate>
        {
            new TaskTemplate
            {
                Name = "Лекция",
                Description = "Посетить лекцию по предмету",
                DefaultPriority = 1,
                DefaultDuration = TimeSpan.FromHours(1.5),
                IsRecurring = false
            },
            new TaskTemplate
            {
                Name = "Лабораторная работа",
                Description = "Выполнить лабораторную работу",
                DefaultPriority = 2,
                DefaultDuration = TimeSpan.FromHours(2),
                IsRecurring = false
            },
            new TaskTemplate
            {
                Name = "Практическое занятие",
                Description = "Практика по заданной теме",
                DefaultPriority = 3,
                DefaultDuration = TimeSpan.FromHours(1),
                IsRecurring = false
            }
        });
            context.SaveChanges();
        }
    }

}
*/