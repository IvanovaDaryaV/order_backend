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
    }
}
