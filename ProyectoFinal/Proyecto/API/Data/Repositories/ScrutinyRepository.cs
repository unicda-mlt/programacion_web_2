using Domain.Models;

namespace Data.Repositories
{
    public class ScrutinyRepository(AppDbContext context) : GenericRepository<Guid, Scrutiny>(context) { }
}
