using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IOrderLineItemsTaxLinesRepository : IRepository<OrderLineItemsTaxLines>
    {
        Task<List<OrderLineItemsTaxLines>> FetchByLineItemIds(List<long> lineItemIds);
    }
    public sealed class OrderLineItemsTaxLinesRepository : Repository<OrderLineItemsTaxLines>, IOrderLineItemsTaxLinesRepository
    {
        public OrderLineItemsTaxLinesRepository(DatabaseContext context) : base(context) { }

        public async Task<List<OrderLineItemsTaxLines>> FetchByLineItemIds(List<long> lineItemIds)
        {
            return await Context.OrderLineItemsTaxLines.Where(w => lineItemIds.Contains(w.OrderLineItemsId)).ToListAsync();
        }
    }
}
