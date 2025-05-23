﻿using AutoMapper;
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
        protected readonly INotificationService _notificationService;

        public BaseService(RMSDbContext context, IMapper mapper, INotificationService notificationService)
        {
            _context = context;
            _mapper = mapper;
            _dbSet = _context.Set<TEntity>();
            _notificationService = notificationService;
        }

        public async Task<List<TViewModel>> GetAllAsync()
        {
            var entities = await _dbSet.ToListAsync();
            return _mapper.Map<List<TViewModel>>(entities);
        }

        public virtual async Task<TViewModel?> GetByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            return entity == null ? null : _mapper.Map<TViewModel>(entity);
        }

        public virtual async Task CreateAsync(TViewModel model)
        {
            var entity = _mapper.Map<TEntity>(model);
            await _dbSet.AddAsync(entity);
            await CreateRelationshipsAsync(entity, model);
            await _context.SaveChangesAsync();
            await _notificationService.NotifyAllAsync($"{typeof(TEntity).Name}Changed", model);
        }

        public virtual async Task UpdateAsync(TViewModel model)
        {
            var entityId = typeof(TViewModel).GetProperty("Id")?.GetValue(model);
            var entity = await _dbSet.FindAsync(entityId);
            if (entity == null) throw new Exception($"{typeof(TEntity).Name} not found");
            _mapper.Map(model, entity);
            await UpdateRelationshipsAsync(entity, model);
            await _context.SaveChangesAsync();
            await _notificationService.NotifyAllAsync($"{typeof(TEntity).Name}Changed", model);
        }

        public virtual async Task<bool> DeleteByIdAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null) return false;
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            await _notificationService.NotifyAllAsync($"{typeof(TEntity).Name}Changed", new { DeletedId = id });
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _dbSet.FindAsync(id) != null;
        }

        // 🔹 Protected virtual - can be overridden, but not required
        protected virtual Task CreateRelationshipsAsync(TEntity entity, TViewModel model)
        {
            return Task.CompletedTask;
        }

        protected virtual Task UpdateRelationshipsAsync(TEntity entity, TViewModel model)
        {
            return Task.CompletedTask;
        }
    }
}
