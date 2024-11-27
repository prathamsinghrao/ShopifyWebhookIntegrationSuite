using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IOrdersShippingLinesRepository : IRepository<OrdersShippingLines>
    {
        Task<List<OrdersShippingLines>> FindByOrderId(long orderId);
    }
    public sealed class OrdersShippingLinesRepository : Repository<OrdersShippingLines>, IOrdersShippingLinesRepository
    {
        public OrdersShippingLinesRepository(DatabaseContext context) : base(context) { }
        public async Task<List<OrdersShippingLines>> FindByOrderId(long orderId)
        {
            return await Context.OrdersShippingLines.Where(w => w.OrderId == orderId).ToListAsync();
        }
    }
}
