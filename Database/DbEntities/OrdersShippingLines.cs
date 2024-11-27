namespace Database.DbEntities
{
    public class OrdersShippingLines
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public string? DiscountedPriceSetPresentmentMoneyCurrencyCode { get; set; }
        public string? Code { get; set; }
        public string? Title { get; set; }
        public string? PriceSetShopMoneyAmount { get; set; }
    }
}
