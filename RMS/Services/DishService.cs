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

        //public override async Task CreateAsync(DishViewModel model)
        //{
        //    var dish = _mapper.Map<Dish>(model);

        //    await _dbSet.AddAsync(dish);
        //    await _context.SaveChangesAsync();

        //    if (model.Ingredients != null && model.Ingredients.Any())
        //    {
        //        dish.DishIngredients = model.Ingredients.Select(i => new DishIngredient
        //        {
        //            DishId = dish.Id,
        //            IngredientId = i.IngredientId,
        //            QuantityNeeded = i.QuantityNeeded
        //        }).ToList();
        //    }

        //    _context.Update(dish);
        //    await _context.SaveChangesAsync();
        //}

        protected override async Task CreateRelationshipsAsync(Dish entity, DishViewModel model)
        {
            if (model.Ingredients != null && model.Ingredients.Any())
            {
                var dishIngredients = model.Ingredients
                    .Select(i => new DishIngredient
                    {
                        IngredientId = i.IngredientId,
                        QuantityNeeded = i.QuantityNeeded
                    }).ToList();
                entity.DishIngredients = dishIngredients;
            }
        }

        protected override async Task UpdateRelationshipsAsync(Dish entity, DishViewModel model)
        {
            if (model.Ingredients == null || !model.Ingredients.Any())
            {
                await _context.DishIngredients
                    .Where(di => di.DishId == entity.Id)
                    .ExecuteDeleteAsync();

                var newIngredients = model.Ingredients.Select(i => new DishIngredient
                {
                    //DishId = entity.Id,
                    IngredientId = i.IngredientId,
                    QuantityNeeded = i.QuantityNeeded
                }).ToList();

                await _context.DishIngredients.AddRangeAsync(newIngredients);
            }
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
