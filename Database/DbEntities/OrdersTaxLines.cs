namespace Database.DbEntities
{
    public class OrdersTaxLines
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public required string Title { get; set; }
        public decimal Rate { get; set; }
        public decimal Price { get; set; }
    }
}
