using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace RMS.Services
{
    public interface IOrderService : IBaseService<OrderViewModel, Order> { }

    public class OrderService : BaseService<OrderViewModel, Order>, IOrderService
    {
        public OrderService(RMSDbContext context, IMapper mapper)
            : base(context, mapper)
        {
        }
    }

}
