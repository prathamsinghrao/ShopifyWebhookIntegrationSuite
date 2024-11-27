namespace Database.DbEntities
{
    public class Log
    {
        public int Id { get; set; }
        public required string Message { get; set; }
        public string Exception { get; set; }
        public string RequestBody { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
