using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IOrdersRepository : IRepository<Orders>
    {
        Task<Orders> FindById(long orderId);
    }
    public sealed class OrdersRepository : Repository<Orders>, IOrdersRepository
    {
        public OrdersRepository(DatabaseContext context) : base(context) { }
        public async Task<Orders> FindById(long orderId)
        {
            return await Context.Order.FirstOrDefaultAsync(f => f.OrderId == orderId);
        }
    }
}
