using Data.Repositories.Interfaces;
using Domain.API;
using Domain.API.Interfaces;
using Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Data.Repositories
{
    public class GenericRepository<IdType, Entity>(AppDbContext context) : IGenericRepository<IdType, Entity> where Entity : BaseEntity<IdType>
    {
        internal readonly AppDbContext _context = context;
        internal readonly DbSet<Entity> _set = context.Set<Entity>();

        public async Task<Entity> Create(Entity entity)
        {
            _context.Entry(entity).State = EntityState.Added;

            entity.CreatedAt = DateTime.Now;

            _set.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }

        public async Task<List<Entity>> CreateMultiple(IEnumerable<Entity> records)
        {
            foreach (Entity record in records)
            {
                record.UpdatedAt = DateTime.Now;
                _context.Entry(record).State = EntityState.Added;
            }

            await _context.SaveChangesAsync();

            return [.. records];
        }

        public async Task Edit(Entity entity)
        {
            entity.UpdatedAt = DateTime.Now;

            Entity? dbEntity = await _set.Where(x => x.Id!.Equals(entity.Id)).FirstOrDefaultAsync();

            if (dbEntity == null)
            {
                return;
            }

            _context.Entry(dbEntity).CurrentValues.SetValues(entity);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteById(IdType Id)
        {
            Entity? dbEntity = await _set.Where(x => x.Id!.Equals(Id)).FirstOrDefaultAsync();

            if (dbEntity == null)
            {
                return;
            }

            _context.Entry(dbEntity).State = EntityState.Deleted;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteWhere(Expression<Func<Entity, bool>> filter)
        {
            var dbEntities = await _set.Where(filter).ToListAsync();

            if (dbEntities == null)
            {
                return;
            }

            foreach (var dbEntity in dbEntities)
            {
                _context.Entry(dbEntity).State = EntityState.Deleted;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Entity?> GetOneByFilter(Expression<Func<Entity, bool>> filter)
        {
            return await _set.Where(filter).FirstOrDefaultAsync();
        }

        public async Task<Entity?> GetOneByFilter(Expression<Func<Entity, bool>> filter, string include)
        {
            return await _set.Include(include).Where(filter).FirstOrDefaultAsync();
        }

        public async Task<Entity?> GetOneByFilter(Expression<Func<Entity, bool>> filter, string[] include)
        {
            IQueryable<Entity> query = _set;

            foreach (string item in include)
            {
                query = query.Include(item);
            }

            return await query.Where(filter).FirstOrDefaultAsync();
        }

        public async Task<Entity?> GetOneByFilter(Expression<Func<Entity, bool>> filter, Expression<Func<Entity, object>> orderByDescending)
        {
            return await _set.Where(filter).OrderByDescending(orderByDescending).FirstOrDefaultAsync();
        }

        public async Task<Entity?> GetById(IdType Id)
        {
            return await _set.IgnoreAutoIncludes().Where(x => x.Id!.Equals(Id)).FirstOrDefaultAsync();
        }

        public async Task<Entity?> GetById(IdType Id, string include)
        {
            return await _set.Include(include).Where(x => x.Id!.Equals(Id)).FirstOrDefaultAsync();
        }

        public async Task<Entity?> GetById(IdType Id, string[] include)
        {
            IQueryable<Entity> query = _set.AsNoTracking();

            foreach (string item in include)
            {
                query = query.Include(item);
            }

            return await query.Where(x => x.Id!.Equals(Id)).FirstOrDefaultAsync();
        }

        public async Task<IList<Entity>> GetAllNoPagination(Expression<Func<Entity, bool>> filter, string? include = null)
        {
            List<Entity> data;

            if (include != null)
            {
                data = await _set
                .Include(include)
                .Where(filter)
                .ToListAsync();
            }
            else
            {
                data = await _set
                .Where(filter)
                .ToListAsync();
            }

            return data;
        }

        public async Task<IPaginationResponse<Entity>> GetAll(int? pageArg, byte? pageSizeArg)
        {
            int page = pageArg ?? 1;
            byte pageSize = pageSizeArg ?? 10;

            int count = await _set.CountAsync();

            if (count == 0)
            {

                return new PaginationResponse<Entity>()
                {
                    Pagination = new Pagination(),
                    Data = []
                };

            }

            int pages = (int)Math.Ceiling(count / (double)pageSize);

            var data = await _set
                .IgnoreAutoIncludes()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(x => x.Id)
                .ToListAsync();

            return new PaginationResponse<Entity>()
            {
                Pagination = new Pagination()
                {
                    Pages = pages,
                    Records = count,
                    CurrentPage = page,
                    PrevPage = page > 1 ? page - 1 : 0,
                    NextPage = page < pages ? page + 1 : 0
                },
                Data = data
            };
        }

        public async Task<IPaginationResponse<Entity>> GetAll(string include, int? pageArg, byte? pageSizeArg)
        {
            int page = pageArg ?? 1;
            byte pageSize = pageSizeArg ?? 10;

            int count = await _set.CountAsync();

            if (count == 0)
            {

                return new PaginationResponse<Entity>()
                {
                    Pagination = new Pagination(),
                    Data = []
                };

            }

            int pages = (int)Math.Ceiling(count / (double)pageSize);

            var data = await _set
                .Include(include)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(x => x.Id)
                .ToListAsync();


            return new PaginationResponse<Entity>()
            {
                Pagination = new Pagination()
                {
                    Pages = pages,
                    Records = count,
                    CurrentPage = page,
                    PrevPage = page > 1 ? page - 1 : 0,
                    NextPage = page < pages ? page + 1 : 0
                },
                Data = data
            };
        }

        public async Task<IPaginationResponse<Entity>> GetAll(string[] include, int? pageArg, byte? pageSizeArg)
        {
            int page = pageArg ?? 1;
            byte pageSize = pageSizeArg ?? 10;

            int count = await _set.CountAsync();

            if (count == 0)
            {

                return new PaginationResponse<Entity>()
                {
                    Pagination = new Pagination(),
                    Data = []
                };

            }

            int pages = (int)Math.Ceiling(count / (double)pageSize);

            IQueryable<Entity> query = _set;

            foreach (string item in include)
            {
                query = query.Include(item);
            }

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(x => x.Id)
                .ToListAsync();


            return new PaginationResponse<Entity>()
            {
                Pagination = new Pagination()
                {
                    Pages = pages,
                    Records = count,
                    CurrentPage = page,
                    PrevPage = page > 1 ? page - 1 : 0,
                    NextPage = page < pages ? page + 1 : 0
                },
                Data = data
            };
        }

        public async Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter)
        {
            int count = await _set.Where(filter).CountAsync();

            if (count == 0)
            {

                return new PaginationResponse<Entity>()
                {
                    Pagination = new Pagination(),
                    Data = []
                };

            }

            var data = await _set
                .Where(filter)
                .OrderBy(x => x.Id)
                .ToListAsync();


            return new PaginationResponse<Entity>()
            {
                Pagination = new Pagination()
                {
                    Pages = 1,
                    Records = count,
                    CurrentPage = 1,
                    PrevPage = 0,
                    NextPage = 0
                },
                Data = data
            };
        }

        public async Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter, string? include)
        {
            int count = await _set.Where(filter).CountAsync();

            if (count == 0)
            {

                return new PaginationResponse<Entity>()
                {
                    Pagination = new Pagination(),
                    Data = []
                };

            }

            List<Entity> data;

            if (include != null)
            {
                data = await _set
                .Include(include)
                .Where(filter)
                .OrderBy(x => x.Id)
                .ToListAsync();
            }
            else
            {
                data = await _set
                .Where(filter)
                .OrderBy(x => x.Id)
                .ToListAsync();
            }

            return new PaginationResponse<Entity>()
            {
                Pagination = new Pagination()
                {
                    Pages = 1,
                    Records = count,
                    CurrentPage = 1,
                    PrevPage = 0,
                    NextPage = 0
                },
                Data = data
            };
        }

        public async Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter, int? pageArg, byte? pageSizeArg)
        {
            int page = pageArg ?? 1;
            byte pageSize = pageSizeArg ?? 10;

            int count = await _set.Where(filter).CountAsync();

            if (count == 0)
            {

                return new PaginationResponse<Entity>()
                {
                    Pagination = new Pagination(),
                    Data = []
                };

            }

            int pages = (int)Math.Ceiling(count / (double)pageSize);

            var data = await _set
                .Where(filter)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderBy(x => x.Id)
                .ToListAsync();


            return new PaginationResponse<Entity>()
            {
                Pagination = new Pagination()
                {
                    Pages = pages,
                    Records = count,
                    CurrentPage = page,
                    PrevPage = page > 1 ? page - 1 : 0,
                    NextPage = page < pages ? page + 1 : 0
                },
                Data = data
            };
        }

        public async Task<IPaginationResponse<TResult>> GetAll<TResult, TOrder>(
            Expression<Func<Entity, bool>> filter,
            Func<Entity, TResult> selector,
            Expression<Func<Entity, TOrder>> orderBy,
            int? pageArg, byte? pageSizeArg
        )
        {
            int page = pageArg ?? 1;
            byte pageSize = pageSizeArg ?? 10;

            int count = await _set.Where(filter).CountAsync();

            if (count == 0)
            {

                return new PaginationResponse<TResult>()
                {
                    Pagination = new Pagination(),
                    Data = []
                };

            }

            int pages = (int)Math.Ceiling(count / (double)pageSize);

            var data = _set
                .Where(filter)
                .OrderBy(orderBy)
                .Select(selector)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginationResponse<TResult>()
            {
                Pagination = new Pagination()
                {
                    Pages = pages,
                    Records = count,
                    CurrentPage = page,
                    PrevPage = page > 1 ? page - 1 : 0,
                    NextPage = page < pages ? page + 1 : 0
                },
                Data = data
            };
        }

        public async Task<IPaginationResponse<TResult>> GetAll<TResult, TOrder>(
            Expression<Func<Entity, bool>> filter,
            string[] include,
            Func<Entity, TResult> selector,
            Expression<Func<Entity, TOrder>> orderBy,
            int? pageArg, byte? pageSizeArg
        )
        {
            int page = pageArg ?? 1;
            byte pageSize = pageSizeArg ?? 10;

            int count = await _set.Where(filter).CountAsync();

            if (count == 0)
            {

                return new PaginationResponse<TResult>()
                {
                    Pagination = new Pagination(),
                    Data = []
                };

            }

            int pages = (int)Math.Ceiling(count / (double)pageSize);

            var query = _set.Where(filter);

            foreach (string item in include)
            {
                query = query.Include(item);
            }

            var data = query
                .OrderBy(orderBy)
                .Select(selector)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginationResponse<TResult>()
            {
                Pagination = new Pagination()
                {
                    Pages = pages,
                    Records = count,
                    CurrentPage = page,
                    PrevPage = page > 1 ? page - 1 : 0,
                    NextPage = page < pages ? page + 1 : 0
                },
                Data = data
            };
        }

        public async Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter, string? include, int? pageArg, byte? pageSizeArg)
        {
            int page = pageArg ?? 1;
            byte pageSize = pageSizeArg ?? 10;

            int count = await _set.AsNoTracking().Where(filter).CountAsync();

            if (count == 0)
            {

                return new PaginationResponse<Entity>()
                {
                    Pagination = new Pagination(),
                    Data = []
                };

            }

            int pages = (int)Math.Ceiling(count / (double)pageSize);

            List<Entity> data;

            if (include != null)
            {
                data = await _set
                    .Include(include)
                    .Where(filter)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            }
            else
            {
                data = await _set
                    .Where(filter)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            }


            return new PaginationResponse<Entity>()
            {
                Pagination = new Pagination()
                {
                    Pages = pages,
                    Records = count,
                    CurrentPage = page,
                    PrevPage = page > 1 ? page - 1 : 0,
                    NextPage = page < pages ? page + 1 : 0
                },
                Data = data
            };
        }

        public async Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter, string[]? include, int? pageArg, byte? pageSizeArg)
        {
            int page = pageArg ?? 1;
            byte pageSize = pageSizeArg ?? 10;

            int count = await _set.Where(filter).CountAsync();

            if (count == 0)
            {

                return new PaginationResponse<Entity>()
                {
                    Pagination = new Pagination(),
                    Data = []
                };

            }

            int pages = (int)Math.Ceiling(count / (double)pageSize);

            List<Entity> data;

            if (include != null)
            {

                IQueryable<Entity> query = _set;

                foreach (string item in include)
                {
                    query = query.Include(item);
                }

                data = await query
                    .Where(filter)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            }
            else
            {
                data = await _set
                    .Where(filter)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .OrderBy(x => x.Id)
                    .ToListAsync();
            }


            return new PaginationResponse<Entity>()
            {
                Pagination = new Pagination()
                {
                    Pages = pages,
                    Records = count,
                    CurrentPage = page,
                    PrevPage = page > 1 ? page - 1 : 0,
                    NextPage = page < pages ? page + 1 : 0
                },
                Data = data
            };
        }

        public async Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter, string[] include, Expression<Func<Entity, int>> orderBy)
        {
            int count = await _set.Where(filter).CountAsync();

            if (count == 0)
            {

                return new PaginationResponse<Entity>()
                {
                    Pagination = new Pagination(),
                    Data = []
                };

            }

            List<Entity> data;

            if (include != null)
            {

                IQueryable<Entity> query = _set;

                foreach (string item in include)
                {
                    query = query.Include(item);
                }

                data = await query
                    .Where(filter)
                    .OrderBy(orderBy)
                    .ToListAsync();
            }
            else
            {
                data = await _set
                    .Where(filter)
                    .OrderBy(orderBy)
                    .ToListAsync();
            }


            return new PaginationResponse<Entity>()
            {
                Pagination = new Pagination()
                {
                    Pages = 1,
                    Records = count,
                    CurrentPage = 1,
                    PrevPage = 0,
                    NextPage = 0
                },
                Data = data
            };
        }

        public async Task<IPaginationResponse<Entity>> GetAll(Expression<Func<Entity, bool>> filter, string[] include, Expression<Func<Entity, int>> orderBy, int? pageArg, byte? pageSizeArg)
        {
            int page = pageArg ?? 1;
            byte pageSize = pageSizeArg ?? 10;

            int count = await _set.Where(filter).CountAsync();

            if (count == 0)
            {

                return new PaginationResponse<Entity>()
                {
                    Pagination = new Pagination(),
                    Data = []
                };

            }

            int pages = (int)Math.Ceiling(count / (double)pageSize);

            List<Entity> data;

            if (include != null)
            {

                IQueryable<Entity> query = _set;

                foreach (string item in include)
                {
                    query = query.Include(item);
                }

                data = await query
                    .Where(filter)
                    .OrderBy(orderBy)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                data = await _set
                    .Where(filter)
                    .OrderBy(orderBy)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }


            return new PaginationResponse<Entity>()
            {
                Pagination = new Pagination()
                {
                    Pages = pages,
                    Records = count,
                    CurrentPage = page,
                    PrevPage = page > 1 ? page - 1 : 0,
                    NextPage = page < pages ? page + 1 : 0
                },
                Data = data
            };
        }

    }

}
