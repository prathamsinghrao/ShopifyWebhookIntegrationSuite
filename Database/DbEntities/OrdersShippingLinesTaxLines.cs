namespace Database.DbEntities
{
    public class OrdersShippingLinesTaxLines
    {
        public long Id { get; set; }
        public long OrdersShippingLinesId { get; set; }
        public string? Title { get; set; }
        public decimal Rate { get; set; }
        public string? PriceSetShopMoneyAmount { get; set; }
    }
}
