using Database.DbEntities;
using Database.EFDbContext;
using Newtonsoft.Json;

namespace Database.Repository.EFRepo
{
    public interface ILogRepository : IRepository<Log>
    {
        Task AddDbErrorLog(string message, string body, Exception? ex);
    }
    public sealed class LogRepository : Repository<Log>, ILogRepository
    {
        public LogRepository(DatabaseContext context) : base(context) { }

        public async Task AddDbErrorLog(string message, string body, Exception? ex)
        {
            Log log = new()
            {
                Message = message,
                RequestBody = body,
                Exception = ex == null ? " " : JsonConvert.SerializeObject(ex),
            };
            await this.InsertAsync(log);
        }
    }
}
