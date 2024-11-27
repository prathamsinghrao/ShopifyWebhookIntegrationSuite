namespace Database.DbEntities
{
    public class Customers
    {
        public long CustomerId { get; set; }
        public required string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Currency { get; set; }
        public string? Phone { get; set; }
        public required string DefaultAddressCountry { get; set; }
        public required string DefaultAddressCity { get; set; }
        public required string DefaultAddressZip { get; set; }
    }
}
