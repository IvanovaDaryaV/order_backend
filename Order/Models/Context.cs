using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Order.Models
{
    [Table("contexts")]
    public class Context
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ContextId { get; set; }
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
