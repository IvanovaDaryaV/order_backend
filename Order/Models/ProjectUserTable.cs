using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Order.Models
{
    // Промежуточная таблица для связи многие ко многим между пользователем и проектами
    [Table("ProjectUserTable")]
    public class ProjectUser
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [Column(TypeName = "uuid")]
        public Guid? UserId { get; set; }
        [Required]
        public int? ProjectId { get; set; }
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }
    }
}
