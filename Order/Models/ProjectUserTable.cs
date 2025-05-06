using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Order.Models
{
    // Промежуточная таблица для связи многие ко многим между пользователем и проектами
    [Table("projects_users")]
    public class ProjectUser
    {
        //[Key]
        //[Required]
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        //public int Id { get; set; }
        [Required]
        [Key]
        [Column(TypeName = "uuid")]
        public Guid UserId { get; set; }
        [Required]
        [Key]
        public int ProjectId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User User { get; set; }
        [ForeignKey("ProjectId")]
        public Project Project { get; set; }
    }
}
