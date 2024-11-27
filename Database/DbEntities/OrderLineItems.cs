namespace Database.DbEntities
{
    public class OrderLineItems
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public required string Title { get; set; }
        public string? PreTaxPriceSetPresentmentMoneyCurrencyCode { get; set; }
        public required string Vendor { get; set; }
        public required string Name { get; set; }
        public long ProductId { get; set; }
        public required string Sku { get; set; }
        public bool GiftCard { get; set; }
        public bool Taxable { get; set; }
        public int Quantity { get; set; }
        public decimal PreTaxPrice { get; set; }
        public decimal Price { get; set; }
    }
}
