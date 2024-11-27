using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IRefundsOrderAdjustmentsRepository : IRepository<RefundsOrderAdjustments>
    {
        Task<List<RefundsOrderAdjustments>> FindByRefundIds(List<long> refundIds);
    }
    public sealed class RefundsOrderAdjustmentsRepository : Repository<RefundsOrderAdjustments>, IRefundsOrderAdjustmentsRepository
    {
        public RefundsOrderAdjustmentsRepository(DatabaseContext context) : base(context) { }
        public async Task<List<RefundsOrderAdjustments>> FindByRefundIds(List<long> refundIds)
        {
            return await Context.RefundsOrderAdjustments.Where(w => refundIds.Contains(w.OrderRefundId)).ToListAsync();
        }
    }
}
