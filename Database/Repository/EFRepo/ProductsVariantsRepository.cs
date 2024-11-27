using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IProductsVariantsRepository : IRepository<ProductsVariants>
    {
        Task<List<ProductsVariants>> FindByVariantIds(List<long> variantIds);
    }
    public sealed class ProductsVariantsRepository : Repository<ProductsVariants>, IProductsVariantsRepository
    {
        public ProductsVariantsRepository(DatabaseContext context) : base(context) { }

        public async Task<List<ProductsVariants>> FindByVariantIds(List<long> variantIds)
        {
            return await Context.ProductsVariants.Where(w => variantIds.Contains(w.Id)).ToListAsync();
        }
    }
}
