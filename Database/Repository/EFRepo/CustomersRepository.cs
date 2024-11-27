using Database.EFDbContext;
using Database.DbEntities;
using Microsoft.EntityFrameworkCore;

namespace Database.Repository.EFRepo
{
    public interface ICustomersRepository : IRepository<Customers>
    {
        Task<Customers> FindById(long customersId);
    }
    public sealed class CustomersRepository : Repository<Customers>, ICustomersRepository
    {
        public CustomersRepository(DatabaseContext context) : base(context) { }

        public async Task<Customers> FindById(long customersId)
        {
            return await Context.Customer.FirstOrDefaultAsync(f => f.CustomerId == customersId);
        }
    }
}
