using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Order.Models.DTO
{
    public class NoteDto
    {
        [Required]
        public string Text { get; set; }
        [Required]
        [Column(TypeName = "uuid")]
        public Guid? UserId { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? DateCreated { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? LastEdited { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
        // поле для перевода в раздел someday-maybe / next actions / waiting for
        // может принимать значения 
        public string? Tag { get; set; }
        public bool? IsDone { get; set; }

        // заметка может быть привязана к проекту
        public int? ProjectId { get; set; }
        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }
    }
}
