using RMS.Data;
using RMS.Data.Entities;
using RMS.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace RMS.Services
{
    public interface IPaymentService : IBaseService<PaymentViewModel, Payment> { }

    public class PaymentService : BaseService<PaymentViewModel, Payment>, IPaymentService
    {
        public PaymentService(RMSDbContext context, IMapper mapper, INotificationService notificationService)
            : base(context, mapper, notificationService)
        {
        }

        public override async Task<PaymentViewModel?> GetByIdAsync(int id)
        {
            var payment = await _context.Payments
                .Include(p => p.Order)
                    .ThenInclude(o => o.Table)
                .FirstOrDefaultAsync(p => p.Id == id);

            return payment == null ? null : _mapper.Map<PaymentViewModel>(payment);
        }

        // Custom logic nếu cần
    }
}
