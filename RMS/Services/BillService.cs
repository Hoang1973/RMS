using RMS.Data;
using RMS.Data.Entities;
using RMS.Models;
using AutoMapper;

namespace RMS.Services
{
    public interface IBillService : IBaseService<BillViewModel, Bill> { }

    public class BillService : BaseService<BillViewModel, Bill>, IBillService
    {
        public BillService(RMSDbContext context, IMapper mapper) : base(context, mapper) { }
        // Custom logic nếu cần
    }
}
