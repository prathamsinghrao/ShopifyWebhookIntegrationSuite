using Database.DbEntities;
using Database.EFDbContext;

namespace Database.Repository.EFRepo
{
    public interface IProductsTrackingRepository : IRepository<ProductsTracking>
    {
    }
    public sealed class ProductsTrackingRepository : Repository<ProductsTracking>, IProductsTrackingRepository
    {
        public ProductsTrackingRepository(DatabaseContext context) : base(context) { }
    }
}
