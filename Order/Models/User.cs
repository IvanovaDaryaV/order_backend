using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Order.Models
{
    public class User
    {
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

        public ICollection<Task> Tasks { get; set; } //коллекция задач
        public ICollection<Project> Projects { get; set; } //коллекция проектов
        public ICollection<Event> Events { get; set; } //коллекция событий

    }
}
