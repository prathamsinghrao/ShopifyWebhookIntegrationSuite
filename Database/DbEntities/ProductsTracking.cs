namespace Database.DbEntities
{
    public class ProductsTracking
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
