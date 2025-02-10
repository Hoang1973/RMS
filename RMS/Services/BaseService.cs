using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RMS.Data;

namespace RMS.Services
{
    public class BaseService<TViewModel, TEntity> : IBaseService<TViewModel, TEntity>
        where TViewModel : class
        where TEntity : class
    {
        protected readonly RMSDbContext _context;
        protected readonly IMapper _mapper;
        protected readonly DbSet<TEntity> _dbSet;

        public BaseService(RMSDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _dbSet = _context.Set<TEntity>();
        }

        public async Task<List<TViewModel>> GetAllAsync()
        {
            var entities = await _dbSet.ToListAsync();
            return _mapper.Map<List<TViewModel>>(entities);
        }

        public async Task<TViewModel?> GetByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity == null ? null : _mapper.Map<TViewModel>(entity);
        }

        public async Task CreateAsync(TViewModel model)
        {
            var entity = _mapper.Map<TEntity>(model);
            _dbSet.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TViewModel model)
        {
            var entityId = typeof(TViewModel).GetProperty("Id")?.GetValue(model);
            var entity = await _dbSet.FindAsync(entityId);
            //var entity = await _dbSet.FindAsync(_mapper.Map<TEntity>(model));
            if (entity == null) throw new Exception($"{typeof(TEntity).Name} not found");
            _mapper.Map(model, entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return false;

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.FindAsync(id) != null;
        }
    }
}
