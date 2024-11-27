namespace Database.DbEntities
{
    public class OrdersLineItemsDiscountAllocations
    {
        public long Id { get; set; }
        public long OrderLineItemsId { get; set; }
        public required string Amount { get; set; }
    }
}
