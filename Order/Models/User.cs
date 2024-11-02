using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Order.Models
{
    public class User
    {
        public User()
        {
            Events = new List<Event>();
            Tasks = new List<Task>();
            Projects = new List<Project>();
        }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string PasswordHash { get; set; }

        public ICollection<Task> Tasks { get; set; } = new List<Task>(); //коллекция задач
        public ICollection<Project> Projects { get; set; } = new List<Project>(); //коллекция проектов
        public ICollection<Event> Events { get; set; } = new List<Event>(); //коллекция событий

    }
}
