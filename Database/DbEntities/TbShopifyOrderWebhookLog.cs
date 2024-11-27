namespace Database.DbEntities
{
    public class TbShopifyOrderWebhookLog
    {
        public int Id { get; set; }
        public required string Message { get; set; }
        public required string Exception { get; set; }
        public required string RequestBody { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
