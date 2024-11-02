using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Order.Models
{
    public class Context
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Place { get; set; } //адрес из Google Maps

        public ICollection<Task> Tasks { get; set; } //коллекция задач
        public ICollection<Event> Events { get; set; } //коллекция событий
        public ICollection<Project> Projects { get; set; } //коллекция проектов
    }
}
