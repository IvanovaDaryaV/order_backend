using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Order.Models
{
    [Table("users")]
    public class User
    {
        public User()
        {
            Events = new List<Event>();
            Tasks = new List<Task>();
            ProjectUsers = new List<ProjectUser>();
            Contexts = new List<Context>();
            Notes = new List<Note>();
        }

        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public string? PasswordHash { get; set; }
        //[Required]
        //public int ProjectUserId { get; set; }
        public ICollection<Task>? Tasks { get; set; } = new List<Task>();
        public List<ProjectUser>? ProjectUsers { get; set; } = new List<ProjectUser>(); // промежуточная таблица для связи с проектами
        public ICollection<Event>? Events { get; set; } = new List<Event>();
        public ICollection<Context>? Contexts { get; set; } = new List<Context>();
        public ICollection<Note>? Notes { get; set; } = new List<Note>();

    }
}
