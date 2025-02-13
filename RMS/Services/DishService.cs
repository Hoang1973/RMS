using AutoMapper;
using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;
using Microsoft.EntityFrameworkCore;

namespace RMS.Services
{
    public interface IDishService : IBaseService<DishViewModel, Dish> { }

    public class DishService : BaseService<DishViewModel, Dish>, IDishService
    {
        public DishService(RMSDbContext context, IMapper mapper)
            : base(context, mapper)
        {
        }

        public override async Task CreateAsync(DishViewModel model)
        {
            var dish = _mapper.Map<Dish>(model);

            if (model.Ingredients != null && model.Ingredients.Any())
            {
                dish.DishIngredients = model.Ingredients.Select(i => new DishIngredient
                {
                    DishId = dish.Id,
                    IngredientId = i.IngredientId,
                    QuantityNeeded = i.QuantityNeeded
                }).ToList();
            }

            await _dbSet.AddAsync(dish);
            await _context.SaveChangesAsync();
        }

        public override async Task UpdateAsync(DishViewModel model)
        {
            var dish = await _dbSet
                .Include(d => d.DishIngredients)
                .FirstOrDefaultAsync(d => d.Id == model.Id);

            if (dish == null) throw new Exception($"{typeof(Dish).Name} not found");

            _mapper.Map(model, dish); // ✅ Keeps the mapping consistent

            // Update dish ingredients
            if (model.Ingredients != null)
            {
                // Remove old ingredients
                _context.DishIngredients.RemoveRange(dish.DishIngredients);

                // Add new ingredients
                dish.DishIngredients = model.Ingredients.Select(i => new DishIngredient
                {
                    DishId = dish.Id,
                    IngredientId = i.IngredientId,
                    QuantityNeeded = i.QuantityNeeded
                }).ToList();
            }

            await _context.SaveChangesAsync();
        }

        public override async Task<DishViewModel> GetByIdAsync(int id)
        {
            var dish = await _context.Dishes
        .Include(d => d.DishIngredients)
            .ThenInclude(di => di.Ingredient)
        .FirstOrDefaultAsync(d => d.Id == id);

            if (dish == null) return null;

            var viewModel = _mapper.Map<DishViewModel>(dish);
            viewModel.Ingredients = dish.DishIngredients
                .Select(di => new DishViewModel.IngredientItem
                {
                    IngredientId = di.IngredientId,
                    QuantityNeeded = di.QuantityNeeded,
                    IngredientName = di.Ingredient.Name,
                    IngredientType = di.Ingredient.Type,
                    IngredientUnit = di.Ingredient.Unit
                }).ToList();

            return viewModel;
        }
    }
}
