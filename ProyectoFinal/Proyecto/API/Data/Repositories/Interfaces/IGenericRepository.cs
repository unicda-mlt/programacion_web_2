using Domain.API.Interfaces;
using Domain.Models;
using System.Linq.Expressions;

namespace Data.Repositories.Interfaces
{

    public interface IGenericRepository<IdType, Entity> where Entity : BaseEntity<IdType>
    {

        Task<Entity> Create(Entity entity);

        Task<List<Entity>> CreateMultiple(IEnumerable<Entity> records);

        Task Edit(Entity entity);

        Task DeleteById(IdType Id);

        Task DeleteWhere(Expression<Func<Entity, bool>> filter);

        Task<Entity?> GetOneByFilter(Expression<Func<Entity, bool>> filter);

        Task<Entity?> GetOneByFilter(Expression<Func<Entity, bool>> filter, string include);

        Task<Entity?> GetOneByFilter(Expression<Func<Entity, bool>> filter, string[] include);

        Task<Entity?> GetOneByFilter(Expression<Func<Entity, bool>> filter, Expression<Func<Entity, object>> orderByDescending);

        Task<Entity?> GetById(IdType Id);

        Task<Entity?> GetById(IdType Id, string include);

        Task<Entity?> GetById(IdType Id, string[] include);

        Task<IPaginationResponse<Entity>> GetAll(int? page, byte? pageSize);

        Task<IPaginationResponse<Entity>> GetAll(string include, int? page, byte? pageSize);

        Task<IPaginationResponse<Entity>> GetAll(string[] include, int? page, byte? pageSize);

        Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter);

        Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter, string? include);

        Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter, int? page, byte? pageSize);

        Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter, string? include, int? page, byte? pageSize);

        Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter, string[]? include, int? page, byte? pageSize);

        Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter, string[] include, Expression<Func<Entity, int>> orderBy);

        Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter, string[] include, Expression<Func<Entity, int>> orderBy, int? page, byte? pageSize);

    }
}
