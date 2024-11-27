using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IOrdersTaxLinesRepository : IRepository<OrdersTaxLines>
    {
        Task<List<OrdersTaxLines>> FindByOrderId(long orderId);
    }
    public sealed class OrdersTaxLinesRepository : Repository<OrdersTaxLines>, IOrdersTaxLinesRepository
    {
        public OrdersTaxLinesRepository(DatabaseContext context) : base(context) { }

        public async Task<List<OrdersTaxLines>> FindByOrderId(long orderId)
        {
            return await Context.OrdersTaxLines.Where(e => e.OrderId == orderId).ToListAsync();
        }
    }
}
