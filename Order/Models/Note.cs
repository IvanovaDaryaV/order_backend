using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Order.Models
{
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
        public Guid UserId { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? DateCreated { get; set; }
        [Column(TypeName = "timestamp without time zone")]
        public DateTime? LastEdited { get; set; }

        [ForeignKey("UserId")]
        [JsonIgnore]
        public User? User { get; set; }
    }
}
