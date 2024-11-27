namespace Database.DbEntities
{
    public class ProductsVariants
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long? ShopifyProductsId { get; set; }
        public string? Sku { get; set; }
        public string? Title { get; set; }
        public decimal Price { get; set; }
        public long? SkuPosition { get; set; }
        public string? CompareAtPrice { get; set; }
        public string? Option1 { get; set; }
        public string? Option2 { get; set; }
        public string? Option3 { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool Taxable { get; set; }
        public string? Barcode { get; set; }
        public long? Grams { get; set; }
        public decimal Weight { get; set; }
        public string? WeightUnit { get; set; }
        public string? AdminGraphqlApiId { get; set; }
        public decimal Cost { get; set; }
    }
}
