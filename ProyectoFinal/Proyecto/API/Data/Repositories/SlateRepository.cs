using Domain.Models;

namespace Data.Repositories
{
    public class SlateRepository(AppDbContext context) : GenericRepository<Guid, Slate>(context) { }
}
