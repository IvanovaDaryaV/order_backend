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
        public int? Priority { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? PeriodStart { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? PeriodEnd { get; set; }
        public bool? IsPrivate { get; set; }
        public int? ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }
        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
        public List<int>? TaskIds { get; set; } // Список привязанных задач (по id)
    }
    
}
