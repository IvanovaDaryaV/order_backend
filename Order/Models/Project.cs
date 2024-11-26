using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Order.Models
{
    public class Project
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string? Description { get; set; }
        public int? Priority { get; set; }
        public int? ContextId {  get; set; }
        public DateOnly? HardDeadline { get; set; }
        public DateOnly? SoftDeadline { get; set; }
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public bool Status { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; } //навигационное свойство
        [ForeignKey("ContextId")]
        public Context? Context { get; set; } //навигационное свойство
    }
}
