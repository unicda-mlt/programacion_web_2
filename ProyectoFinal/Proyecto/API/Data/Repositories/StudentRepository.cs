using Domain.Models;

namespace Data.Repositories
{
    public class StudentRepository(AppDbContext context) : GenericRepository<Guid, Student>(context) { }
}
