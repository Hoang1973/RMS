using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace RMS.Services
{
    public interface IIngredientService : IBaseService<IngredientViewModel, Ingredient>
    {
        Task<IngredientViewModel> ImportAsync(int id, int quantity);
    }

    public class IngredientService : BaseService<IngredientViewModel, Ingredient>, IIngredientService
    {
        public IngredientService(RMSDbContext context, IMapper mapper)
            : base(context, mapper)
        {
        }

        public async Task<IngredientViewModel> ImportAsync(int id, int quantity)
        {
            var ingredient = await _context.Set<Ingredient>().FindAsync(id);
            if (ingredient == null)
            {
                throw new KeyNotFoundException($"Ingredient with ID {id} not found");
            }

            ingredient.StockQuantity += quantity;

            await _context.SaveChangesAsync();
            return _mapper.Map<IngredientViewModel>(ingredient);
        }
    }
}
