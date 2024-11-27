using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IOrdersShippingLinesTaxLinesRepository : IRepository<OrdersShippingLinesTaxLines>
    {
        Task<List<OrdersShippingLinesTaxLines>> FindByShippingLinesId(List<long> ordersShippingLinesIds);
    }
    public sealed class OrdersShippingLinesTaxLinesRepository : Repository<OrdersShippingLinesTaxLines>, IOrdersShippingLinesTaxLinesRepository
    {
        public OrdersShippingLinesTaxLinesRepository(DatabaseContext context) : base(context) { }

        public async Task<List<OrdersShippingLinesTaxLines>> FindByShippingLinesId(List<long> ordersShippingLinesIds)
        {
            return await Context.OrdersShippingLinesTaxLines.Where(w => ordersShippingLinesIds.Contains(w.OrdersShippingLinesId)).ToListAsync();
        }
    }
}
