using Database.DbEntities;
using Database.EFDbContext;

namespace Database.Repository.DbEntityRepo
{
    public interface IShopifyOrderWebhookLogRepository : IRepository<TbShopifyOrderWebhookLog>
    {
    }
    public sealed class ShopifyOrderWebhookLogRepository : Repository<TbShopifyOrderWebhookLog>, IShopifyOrderWebhookLogRepository
    {
        public ShopifyOrderWebhookLogRepository(DatabaseContext context) : base(context) { }
    }
}
