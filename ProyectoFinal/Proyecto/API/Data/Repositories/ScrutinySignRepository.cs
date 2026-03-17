using Domain.Models;

namespace Data.Repositories
{
    public class ScrutinySignRepository(AppDbContext context) : GenericRepository<Guid, ScrutinySign>(context) { }
}
