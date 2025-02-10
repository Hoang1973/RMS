namespace RMS.Services
{
    public interface IBaseService<TViewModel, TEntity>
        where TViewModel : class
        where TEntity : class
    {
        Task<List<TViewModel>> GetAllAsync();
        Task<TViewModel?> GetByIdAsync(int id);
        Task CreateAsync(TViewModel model);
        Task UpdateAsync(TViewModel model);
        Task<bool> DeleteByIdAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
