namespace Database.DbEntities
{
    public class OrderLineItemsTaxLines
    {
        public long Id { get; set; }
        public long OrderLineItemsId { get; set; }
        public required string Title { get; set; }
        public decimal Rate { get; set; }
        public decimal Price { get; set; }
    }
}
