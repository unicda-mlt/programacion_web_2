using Domain.Models;

namespace Data.Repositories
{
    public class UsuarioRepository(AppDbContext context) : GenericRepository<Guid, User>(context) { }
}
