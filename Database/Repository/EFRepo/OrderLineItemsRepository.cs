using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IOrderLineItemsRepository : IRepository<OrderLineItems>
    {
        Task<List<OrderLineItems>> FindByOrderId(long orderId);
    }
    public sealed class OrderLineItemsRepository : Repository<OrderLineItems>, IOrderLineItemsRepository
    {
        public OrderLineItemsRepository(DatabaseContext context) : base(context) { }
        public async Task<List<OrderLineItems>> FindByOrderId(long orderId)
        {
            return await Context.OrderLineItems.Where(f => f.OrderId == orderId).ToListAsync();
        }
    }
}
