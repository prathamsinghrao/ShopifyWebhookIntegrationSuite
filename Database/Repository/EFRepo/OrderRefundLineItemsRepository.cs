using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IOrderRefundLineItemsRepository : IRepository<OrderRefundLineItems>
    {
        Task<List<OrderRefundLineItems>> FindByRefundIds(List<long> refundIds);
    }
    public sealed class OrderRefundLineItemsRepository : Repository<OrderRefundLineItems>, IOrderRefundLineItemsRepository
    {
        public OrderRefundLineItemsRepository(DatabaseContext context) : base(context) { }
        public async Task<List<OrderRefundLineItems>> FindByRefundIds(List<long> refundIds)
        {
            return await Context.OrderRefundLineItems.Where(w => refundIds.Contains(w.OrderRefundId)).ToListAsync();
        }
    }
}
