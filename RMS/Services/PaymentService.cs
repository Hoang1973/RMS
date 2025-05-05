using RMS.Data;
using RMS.Data.Entities;
using RMS.Models;
using AutoMapper;

namespace RMS.Services
{
    public interface IPaymentService : IBaseService<PaymentViewModel, Payment> { }

    public class PaymentService : BaseService<PaymentViewModel, Payment>, IPaymentService
    {
        public PaymentService(RMSDbContext context, IMapper mapper) : base(context, mapper) { }
        // Custom logic nếu cần
    }
}
