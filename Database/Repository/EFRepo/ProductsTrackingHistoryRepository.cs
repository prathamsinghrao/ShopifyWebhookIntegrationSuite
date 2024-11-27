using Database.DbEntities;
using Database.EFDbContext;

namespace Database.Repository.EFRepo
{
    public interface IProductsTrackingHistoryRepository : IRepository<ProductsTrackingHistory>
    {
    }
    public sealed class ProductsTrackingHistoryRepository : Repository<ProductsTrackingHistory>, IProductsTrackingHistoryRepository
    {
        public ProductsTrackingHistoryRepository(DatabaseContext context) : base(context) { }
    }
}
