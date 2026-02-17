using Domain.Models;

namespace Data.Repositories
{
    public class UserRepository(AppDbContext context) : GenericRepository<Guid, User>(context) { }
}
