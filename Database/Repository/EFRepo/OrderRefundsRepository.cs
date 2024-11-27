using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IOrderRefundsRepository : IRepository<OrderRefunds>
    {
        Task<List<OrderRefunds>> FindByOrderId(long orderId);
    }
    public sealed class OrderRefundsRepository : Repository<OrderRefunds>, IOrderRefundsRepository
    {
        public OrderRefundsRepository(DatabaseContext context) : base(context) { }
        public async Task<List<OrderRefunds>> FindByOrderId(long orderId)
        {
            return await Context.OrderRefunds.Where(w => w.OrderId == orderId).ToListAsync();
        }
    }
}
