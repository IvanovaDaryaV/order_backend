using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Order.Models
{
    public class ScheduleSharing
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ShareId { get; set; }
        public Guid UserId { get; set; }
        public DateTime PeriodStart { get; set; } 
        public DateTime PeriodEnd { get; set; }
        public string PublicLinkToken { get; set; } // Токен публичной ссылки
        public int[] privateEventsId {  get; set; }
    }
}
