using AutoMapper;
using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace RMS.Services
{
    public interface IDiscountService : IBaseService<DiscountViewModel, Discount>
    {
        Task<DiscountViewModel?> GetByIdAsync(int id);
    }

    public class DiscountService : BaseService<DiscountViewModel, Discount>, IDiscountService
    {
        public DiscountService(RMSDbContext context, IMapper mapper)
            : base(context, mapper)
        {
        }

        public async Task<DiscountViewModel?> GetByIdAsync(int id)
        {
            var discount = await _context.Discounts
                .FirstOrDefaultAsync(d => d.Id == id);
            if (discount == null) return null;
            var viewModel = _mapper.Map<DiscountViewModel>(discount);
            return viewModel;
        }
    }
}
