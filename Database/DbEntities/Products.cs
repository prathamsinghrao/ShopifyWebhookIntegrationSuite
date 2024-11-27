namespace Database.DbEntities
{
    public class Products
    {
        public long Id { get; set; }
        public string? Title { get; set; }
        public string? ProductType { get; set; }
        public string? Vendor { get; set; }
        public string? status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
