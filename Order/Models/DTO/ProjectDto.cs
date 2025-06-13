using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Order.Models.DTO
{
    public class ProjectDto
    {
        //[Key]
        //[Required]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int ProjectId { get; set; }
        public string? Description { get; set; }
        public int? Priority { get; set; }
        public int? ContextId { get; set; }
        public DateOnly? HardDeadline { get; set; }
        public DateOnly? SoftDeadline { get; set; }
        [Required]
        public bool Status { get; set; }
        public Guid? OwnerId { get; set; }

        [ForeignKey("ContextId")]
        public Context? Context { get; set; }
        public List<int>? TaskIds { get; set; } // Список привязанных задач (по id)
        public List<int>? NoteIds { get; set; } // Список привязанных заметок (по id)
        public List<string>? Links { get; set; } // Список ссылок (на материалы и тд)
        public ICollection<Event>? Events { get; set; } = new List<Event>();
        public ICollection<Task>? Tasks { get; set; } = new List<Task>();
        public ICollection<Note>? Notes { get; set; } = new List<Note>();
        public List<ProjectUser>? ProjectUsers { get; set; } = new List<ProjectUser>(); // промежуточная таблица для связи с пользователями
        public List<Guid>? UserIds { get; set; }
    }
}
