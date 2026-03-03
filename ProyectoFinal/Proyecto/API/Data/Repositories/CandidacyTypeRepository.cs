using Microsoft.EntityFrameworkCore;
using Domain.Models;

namespace Data.Repositories
{
    public class CandidacyTypeRepository(AppDbContext context) : GenericRepository<short, CandidacyType>(context)
    {
        public async Task<bool> ExistsByNormalizedName(string name, short? excludeId = null)
        {
            var normalizedName = name.Trim().Replace(" ", "").ToLower();

            return await _set.AnyAsync(candidacyType =>
                (excludeId == null || candidacyType.Id != excludeId) &&
                candidacyType.Name.Replace(" ", "").ToLower() == normalizedName
            );
        }
    }
}
