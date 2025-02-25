using AutoMapper;
using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;
using Microsoft.EntityFrameworkCore;

namespace RMS.Services
{
    public interface ITableService : IBaseService<TableViewModel, Table> { }

    public class TableService : BaseService<TableViewModel, Table>, ITableService
    {
        public TableService(RMSDbContext context, IMapper mapper)
            : base(context, mapper)
        {
        }

        
    }
}
