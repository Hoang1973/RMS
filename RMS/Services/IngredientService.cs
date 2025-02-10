using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace RMS.Services
{
    public interface IIngredientService : IBaseService<IngredientViewModel, Ingredient> { }

    public class IngredientService : BaseService<IngredientViewModel, Ingredient>, IIngredientService
    {
        public IngredientService(RMSDbContext context, IMapper mapper)
            : base(context, mapper)
        {
        }
    }

}
