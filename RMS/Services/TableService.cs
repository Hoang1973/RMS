using AutoMapper;
using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;
using Microsoft.EntityFrameworkCore;

namespace RMS.Services
{
    public interface ITableService : IBaseService<TableViewModel, Table> 
    {
        Task<List<Table>> GetAvailableTablesAsync();
    }

    public class TableService : BaseService<TableViewModel, Table>, ITableService
    {
        public TableService(RMSDbContext context, IMapper mapper, INotificationService notificationService)
            : base(context, mapper, notificationService)
        {
        }
        public async Task<List<Table>> GetAvailableTablesAsync()
        {
            return await _context.Set<Table>()
                .Where(t => t.Status == Table.TableStatus.Available)
                .ToListAsync();
        }
    }
}
