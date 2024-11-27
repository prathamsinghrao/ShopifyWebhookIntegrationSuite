using Database.DbEntities;
using Database.EFDbContext;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface IProductRepository : IRepository<Products>
    {
        Task<Products> FindById(long productId);
    }
    public sealed class ProductRepository : Repository<Products>, IProductRepository
    {
        public ProductRepository(DatabaseContext context) : base(context) { }
        public async Task<Products> FindById(long productId)
        {
            return await Context.Product.FirstOrDefaultAsync(f => f.Id == productId);
        }
    }
}
