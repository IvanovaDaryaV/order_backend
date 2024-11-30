using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Order.Models.DTO
{
    public class EventDto
    {
        public string Name { get; set; }
        [Required]
        public bool Status { get; set; }
        public int? ContextId { get; set; }
        public int? Priority { get; set; }
        public DateOnly? CalendarDate { get; set; }
        public bool? IsPrivate { get; set; }
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
        [ForeignKey("ContextId")]
        public Context? Context { get; set; }
        public List<int>? TaskIds { get; set; } // Список привязанных задач (по id)
    }
    
}
