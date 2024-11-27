namespace Database.DbEntities
{
    public class OrdersShippingLinesDiscountAllocations
    {
        public long Id { get; set; }
        public long OrdersShippingLinesId { get; set; }
        public required string Amount { get; set; }
    }
}
