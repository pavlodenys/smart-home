using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SmartHome.Data;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using System.Security.Cryptography;

namespace SmartHome.Logic
{
    public interface IRepository
    {

    }
    public interface IRepository<TEntity,TDto> : IRepository where TEntity : class
    {
        IQueryable<TDto> GetAll();
        TEntity GetById(int id);
        TDto GetMapById(int id); // Rename to GetById
        Task<TDto> Create(TDto entity);
        TEntity Update(TEntity entity);
        TEntity Update(int id, TDto entity); // make Tentity Id

       // TDto UpdateDto(int id, TDto dto);
        Task<int> Delete(TEntity entity);
        Task<int> Delete(int id);
    }
    public class Repository<TEntity, TDto> : IRepository<TEntity, TDto> where TEntity : class
    {
        private Context _dbContext { get; set; }

        private DbSet<TEntity> _set;
        private readonly IMapper _mapper;

        public Repository(Context dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _set = _dbContext.Set<TEntity>();
            _mapper = mapper;
        }

        public TEntity GetById(int id)
        {
            return _dbContext.Set<TEntity>().Find(id);
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

       public IQueryable<TDto> GetAll()
        {
            return _set.ProjectTo<TDto>(_mapper.ConfigurationProvider);
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

        //public TDto UpdateDto(int id, TDto dto)
        //{
        //    var entity = Update(id, dto);

        //    return _mapper.Map<TDto>(entity);
        //}

        public Task<int> Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}