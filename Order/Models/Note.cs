using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Order.Models
{
    [Table("notes")]
    public class Note
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Text { get; set; }
        [Required]
        [Column(TypeName = "uuid")]
        public Guid? UserId { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? DateCreated { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? LastEdited { get; set; }

        // поле для перевода в раздел someday-maybe / next actions / waiting for
        // может принимать значения 
        public string? Tag { get; set; }
        public bool? IsDone { get; set; } // если заметка как задача, она может быть выполнена

        // заметка может быть привязана к проекту
        public int? ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }

    }
}
