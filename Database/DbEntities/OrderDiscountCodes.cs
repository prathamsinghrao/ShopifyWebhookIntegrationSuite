namespace Database.DbEntities
{
    public class OrderDiscountCodes
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public required string Code { get; set; }
    }
}
