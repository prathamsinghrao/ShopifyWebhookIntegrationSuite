using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IOrdersShippingLinesDiscountAllocationsRepository : IRepository<OrdersShippingLinesDiscountAllocations>
    {
        Task<List<OrdersShippingLinesDiscountAllocations>> FindByShippingLinesId(List<long> ordersShippingLinesIds);
    }
    public sealed class OrdersShippingLinesDiscountAllocationsRepository : Repository<OrdersShippingLinesDiscountAllocations>, IOrdersShippingLinesDiscountAllocationsRepository
    {
        public OrdersShippingLinesDiscountAllocationsRepository(DatabaseContext context) : base(context) { }
        public async Task<List<OrdersShippingLinesDiscountAllocations>> FindByShippingLinesId(List<long> ordersShippingLinesIds)
        {
            return await Context.OrdersShippingLinesDiscountAllocations.Where(w => ordersShippingLinesIds.Contains(w.OrdersShippingLinesId)).ToListAsync();
        }
    }
}
