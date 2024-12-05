using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using AutoMapper;

namespace Order.Models
{
    public class Event
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public bool Status { get; set; }
        public int? ContextId { get; set; }
        public int? Priority { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? PeriodStart { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? PeriodEnd { get; set; }
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
