using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Order.Models.DTO
{
    public class ContextDto
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Place { get; set; } //адрес из Google Maps
        [Column(TypeName = "uuid")]
        public Guid? UserId { get; set; }

        public ICollection<Task>? Tasks { get; set; } //коллекция задач
        public ICollection<Project>? Projects { get; set; } //коллекция проектов
        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
    }
}
