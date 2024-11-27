namespace Database.DbEntities
{
    public class ProductsTrackingHistory
    {
        public long Id { get; set; }
        public long ProductId { get; set; }
        public long? ProductsVariantsId { get; set; }
        public string OldStatus { get; set; }
        public string NewStatus { get; set; }
        public string OldVendor { get; set; }
        public string NewVender { get; set; }
        public decimal? OldPrice { get; set; }
        public decimal? NewPrice { get; set; }
        public string OldCompareAtPrice { get; set; }
        public string NewCompareAtPrice { get; set; }
        public DateTime? ProductUpdatedAt { get; set; }
        public DateTime? ProductVariantsUpdatedAt { get; set; }
        public DateTime InsertDatetime { get; set; }
    }
}
