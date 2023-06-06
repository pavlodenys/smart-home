using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SmartHome.Core;
using SmartHome.Core.Extensions;
using SmartHome.Data;
using System.Linq.Expressions;

namespace SmartHome.Logic
{
    public interface IRepository
    {
        SmartHomeDbContext _dbContext { get; set; }
    }
    public interface IRepository<TEntity,TDto> : IRepository where TEntity : class
    {
        Task<IEnumerable<TDto>> GetAll();
        Task<IEnumerable<TDto>> GetAll(Expression<Func<TEntity, bool>> condition, int page = 0, int count = 0);
        TDto GetDtoById(int id);
        TEntity? GetById(int id);
        Task<TDto> GetById(Expression<Func<TEntity, bool>> condition, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);
        TDto GetMapById(int id); // Rename to GetById
        Task<TDto> Create(TDto entity);
        Task<TDto?> Update(int id, TDto entity); // make Tentity Id

        Task<int> Delete(TEntity entity);
    }
    public class Repository<TEntity, TDto> : IRepository<TEntity, TDto> 
        where TEntity : class 
        where TDto : class
    {
        public SmartHomeDbContext _dbContext { get; set; } // todo: make it private back

        private DbSet<TEntity> _set;
        private readonly IMapper _mapper;

        public Repository(SmartHomeDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _set = _dbContext.Set<TEntity>();
            _mapper = mapper;
        }

        public TEntity? GetById(int id)
        {
            return _dbContext.Set<TEntity>().Find(id) ?? null;
        }

        public TDto GetDtoById(int id)
        {
            var item = _dbContext.Set<TEntity>().Find(id);

            return _mapper.Map<TDto>(item);
        }

        public async Task<TDto> GetById(Expression<Func<TEntity, bool>> condition,  Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
        {
            var query = _dbContext.Set<TEntity>().AsQueryable();

            if(include != null)
            {
                query = include(query);
            };
            TEntity? source = await query
                .Where(condition)
                .FirstOrDefaultAsync();
            return _mapper.Map<TDto>(source);
        }

        public TDto GetMapById(int id)
        {
            var entity = _dbContext.Set<TEntity>().Find(id);
            return _mapper.Map<TDto>(entity);
        }

        public async Task<TDto> Create(TDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);
            var result = await _dbContext.Set<TEntity>().AddAsync(entity);

            await _dbContext.SaveChangesAsync();
            var resultEntity = _mapper.Map<TDto>(result.Entity);

            return resultEntity;
        }

        public async Task<int> Delete(TEntity entity)
        {
            var result = _dbContext.Set<TEntity>().Remove(entity);

            var rowsDeleted = await _dbContext.SaveChangesAsync();

            return rowsDeleted;
        }

        public async Task<IEnumerable<TDto>> GetAll()
        {
            var query = _set.Where(_ => true);

            var dtos = query.ProjectTo<TDto>(_mapper.ConfigurationProvider);

            if (typeof(TDto).GetInterfaces().Contains(typeof(IDeleted)))
            {
                return dtos.NotDeleted();
            }

            return await dtos.ToListAsync();
        }

        public async Task<IEnumerable<TDto>> GetAll(Expression<Func<TEntity, bool>> condition, int page = 0, int count = 0)
        {
            var query = _set.Where(condition);

            if (page > 0 && count != 0)
            {
                query = query.Skip(page * count).Take(count);
            }

            var dtos = query.ProjectTo<TDto>(_mapper.ConfigurationProvider);

            if (typeof(TDto).GetInterfaces().Contains(typeof(IDeleted)))
            {
                return dtos.NotDeleted();
            }

            return await dtos.ToListAsync();
        }

        public async Task<TDto?> Update(int id, TDto dto)
        {
            var entity = GetById(id);

            if (entity == null)
                return default;

            _mapper.Map(dto, entity);
           await _dbContext.SaveChangesAsync();

            return dto; //todo: return Id??
        }
    }
}