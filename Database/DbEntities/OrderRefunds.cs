namespace Database.DbEntities
{
    public class OrderRefunds
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public DateTime ProcessedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
