namespace Database.DbEntities
{
    public class RefundsOrderAdjustments
    {
        public long Id { get; set; }
        public long OrderId { get; set; }
        public long OrderRefundId { get; set; }
        public required string Kind { get; set; }
        public decimal Amount { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
