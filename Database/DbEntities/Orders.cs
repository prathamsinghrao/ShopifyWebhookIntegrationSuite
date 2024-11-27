namespace Database.DbEntities
{
    public class Orders
    {
        public long OrderId { get; set; }
        public long OrderNumber { get; set; }
        public long CustomerId { get; set; }
        public required string Currency { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime ProcessedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? FulfillmentStatus { get; set; }
        public string? FinancialStatus { get; set; }
        public long? AppId { get; set; }
        public required string ShippingAddressCity { get; set; }
        public required string ShippingAddressProvinceCode { get; set; }
        public required string ShippingAddressCountry { get; set; }
        public required string BillingAddressCity { get; set; }
        public required string BillingAddressProvinceCode { get; set; }
        public required string BillingAddressCountry { get; set; }
        public int Quantity { get; set; }
        public string? Store { get; set; }
        public string? CurrentTotalDutiesSetShopMoneyAmount { get; set; }
        public string? AdditionalFeesShopMoneyAmount { get; set; }
        public string? PaymentGateway { get; set; }
        public decimal TotalTax { get; set; }
    }
}
