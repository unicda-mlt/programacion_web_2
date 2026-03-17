using Domain.Models;

namespace Data.Repositories
{
    public class VoteRepository(AppDbContext context) : GenericRepository<Guid, Vote>(context) { }
}
