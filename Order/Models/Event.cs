using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public string Status { get; set; }
        public int Context_id { get; set; }
        public int Priority { get; set; }
        public DateOnly CalendarDate { get; set; }
        public bool IsPrivate { get; set; }
        [Required]
        public Guid UserId { get; set; }

        public ICollection<Task> Tasks { get; set; } //коллекция задач
        [ForeignKey("UserId")]
        public User User { get; set; } //навигационное свойство
        [ForeignKey("ContextId")]
        public Context Context { get; set; } //навигационное свойство
    }
}
