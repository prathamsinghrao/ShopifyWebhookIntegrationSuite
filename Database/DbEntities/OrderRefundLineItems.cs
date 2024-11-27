namespace Database.DbEntities
{
    public class OrderRefundLineItems
    {
        public long Id { get; set; }
        public long OrderRefundId { get; set; }
        public long OrderLineItemId { get; set; }
        public int Quantity { get; set; }
        public decimal TotalTax { get; set; }
        public decimal Subtotal { get; set; }
        public decimal LineItemPreTaxPrice { get; set; }
        public decimal LineItemPrice { get; set; }
    }
}
