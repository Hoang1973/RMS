using AutoMapper;
using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;

namespace RMS.Services
{
    public interface IDishService : IBaseService<DishViewModel, Dish> { }

    public class DishService : BaseService<DishViewModel, Dish>, IDishService
    {
        public DishService(RMSDbContext context, IMapper mapper)
            : base(context, mapper)
        {
        }

    }
}
