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
        //IQueryable<TDto> GetAll();
        // IQueryable<TDto> GetAll(Expression<Func<TEntity, bool>> condition, int page = 0, int count = 0);
        TDto GetDtoById(int id);
        TEntity? GetById(int id);
        Task<TDto> GetById(Expression<Func<TEntity, bool>> condition, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);
        TDto GetMapById(int id); // Rename to GetById
        Task<TDto> Create(TDto entity);
        TEntity Update(TEntity entity);
        TEntity? Update(int id, TDto entity); // make Tentity Id

       // TDto UpdateDto(int id, TDto dto);
        Task<int> Delete(TEntity entity);
        /// <summary>
        /// Delete 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
       // Task<int> Delete(int id); // todo: implement it
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

        public TEntity Update(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Delete(TEntity entity)
        {
            var result = _dbContext.Set<TEntity>().Remove(entity);

            var rowsDeleted = await _dbContext.SaveChangesAsync();

            return rowsDeleted;
        }

        //public IQueryable<TDto> GetAll()
        //{
        //    var dtos = _set.ProjectTo<TDto>(_mapper.ConfigurationProvider);

        //    if (((System.Reflection.TypeInfo)typeof(TDto)).ImplementedInterfaces != null && ((System.Reflection.TypeInfo)typeof(TDto)).ImplementedInterfaces.Any(x => x.Name.Contains(nameof(IDeleted))))
        //    {
        //        dtos = dtos.AsEnumerable().NotDeleted().AsQueryable();
        //    }

        //    return dtos;
        //}

        public async Task<IEnumerable<TDto>> GetAll()
        {
            var query = _set.Where(_ => true);

            var dtos = query.ProjectTo<TDto>(_mapper.ConfigurationProvider);

            if (typeof(TDto).GetInterfaces().Contains(typeof(IDeleted)))
            {
                return dtos.NotDeleted();
            }

            return dtos;
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

            return dtos;
        }

        public TEntity? Update(int id, TDto dto)
        {
            var entity = GetById(id);

            if (entity == null)
                return default;

            _mapper.Map(dto, entity);
            _dbContext.SaveChanges();

            return entity;
        }

        //Task<IEnumerable<TDto>> IRepository<TEntity, TDto>.GetAll()
        //{
        //    throw new NotImplementedException();
        //}

        //Task<IEnumerable<TDto>> IRepository<TEntity, TDto>.GetAll(Expression<Func<TEntity, bool>> condition, int page, int count)
        //{
        //    throw new NotImplementedException();
        //}

        //public TDto UpdateDto(int id, TDto dto)
        //{
        //    var entity = Update(id, dto);

        //    return _mapper.Map<TDto>(entity);
        //}

        //public Task<int> Delete(int id)
        //{
        //    var entity = GetById(id);

        //    entity.
        //}
    }
}