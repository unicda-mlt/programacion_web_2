using Domain.Models;

namespace Data.Repositories
{
    public class SlateCandidacyRepository(AppDbContext context) : GenericRepository<Guid, SlateCandidacy>(context) { }
}
