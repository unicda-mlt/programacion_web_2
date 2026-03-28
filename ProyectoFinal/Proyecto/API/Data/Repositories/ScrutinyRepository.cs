using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class ScrutinyRepository(AppDbContext context) : GenericRepository<Guid, Scrutiny>(context)
    {
        public async Task<bool> CanVote(Guid id)
        {
            var currentDate = DateTime.Now;
            
            var dbScrutiny = await _set.Where(scrutiny =>
                scrutiny.Id == id
                && scrutiny.StartDate <= currentDate
                && scrutiny.EndDate >= currentDate
                && scrutiny.StatusId == EScrutinyStatus.OPEN.GetValue()
            ).FirstOrDefaultAsync();

            return dbScrutiny != null;
        }
    }
}
