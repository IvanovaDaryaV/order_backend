using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Order.Models
{
    public class Task
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateOnly? HardDeadline { get; set; }
        public DateOnly? SoftDeadline { get; set; }
        [Required]
        public bool Status { get; set; }
        [JsonIgnore]
        public int? ContextId { get; set; }
        public int? Priority { get; set; }
        [Required]
        [Column(TypeName = "uuid")]
        public Guid UserId { get; set; }
        public int? EventId { get; set; }
        public DateOnly? CallendarDate { get; set; }
        public bool? IsPrivate { get; set; }
        public int? ProjectId { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; } 
        [ForeignKey("ContextId")]
        public Context? Context { get; set; }
        [ForeignKey("EventId")]
        public Event? Event { get; set; } 
    }
}
