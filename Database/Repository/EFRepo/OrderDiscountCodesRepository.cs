using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IOrderDiscountCodesRepository : IRepository<OrderDiscountCodes>
    {
        Task<List<OrderDiscountCodes>> FindByOrderId(long orderId);
    }
    public sealed class OrderDiscountCodesRepository : Repository<OrderDiscountCodes>, IOrderDiscountCodesRepository
    {
        public OrderDiscountCodesRepository(DatabaseContext context) : base(context) { }
        public async Task<List<OrderDiscountCodes>> FindByOrderId(long orderId)
        {
            return await Context.OrderDiscountCodes.Where(w => w.OrderId == orderId).ToListAsync();
        }
    }
}
