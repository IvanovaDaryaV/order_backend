namespace Order.Models
{
    public class ScheduleSharing
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime PeriodStart { get; set; } 
        public DateTime PeriodEnd { get; set; }
        public string PublicLinkToken { get; set; } // Токен публичной ссылки
    }
}
