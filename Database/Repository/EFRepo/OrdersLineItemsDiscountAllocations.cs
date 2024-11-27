using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IOrdersLineItemsDiscountAllocationsRepository : IRepository<OrdersLineItemsDiscountAllocations>
    {
        Task<List<OrdersLineItemsDiscountAllocations>> FindByOrdersLineItemIds(List<long> ordersLineItemIds);
    }
    public sealed class OrdersLineItemsDiscountAllocationsRepository : Repository<OrdersLineItemsDiscountAllocations>, IOrdersLineItemsDiscountAllocationsRepository
    {
        public OrdersLineItemsDiscountAllocationsRepository(DatabaseContext context) : base(context) { }
        public async Task<List<OrdersLineItemsDiscountAllocations>> FindByOrdersLineItemIds(List<long> ordersLineItemIds)
        {
            return await Context.OrdersLineItemsDiscountAllocations.Where(w => ordersLineItemIds.Contains(w.OrderLineItemsId)).ToListAsync();
        }
    }
}
