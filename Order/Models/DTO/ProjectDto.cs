using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Order.Models.DTO
{
    public class ProjectDto
    {
        public string? Description { get; set; }
        public int? Priority { get; set; }
        public int? ContextId { get; set; }
        public DateOnly? HardDeadline { get; set; }
        public DateOnly? SoftDeadline { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public bool Status { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
        [ForeignKey("ContextId")]
        public Context? Context { get; set; }
        public List<int>? TaskIds { get; set; } // Список привязанных задач (по id)
    }
}
