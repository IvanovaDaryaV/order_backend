using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Order.Models
{
    public class User : IdentityUser<Guid>
    {
        public User()
        {
            Events = new List<Event>();
            Tasks = new List<Task>();
            Projects = new List<Project>();
            Contexts = new List<Context>();
        }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public string? PasswordHash { get; set; }

        public ICollection<Task>? Tasks { get; set; } = new List<Task>(); 
        public ICollection<Project>? Projects { get; set; } = new List<Project>(); 
        public ICollection<Event>? Events { get; set; } = new List<Event>();
        public ICollection<Context>? Contexts { get; set; } = new List<Context>();

    }
}
