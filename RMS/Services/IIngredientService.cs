using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;

namespace RMS.Services
{
    public interface IIngredientService
    {
        Task<IngredientViewModel> GetIngredientByIdAsync(int id);
        Task UpdateIngredientAsync(IngredientViewModel model);
    }
    public class IngredientService : IIngredientService
    {
        private readonly RMSDbContext _context;

        public IngredientService(RMSDbContext context)
        {
            _context = context;
        }

        public async Task<IngredientViewModel> GetIngredientByIdAsync(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return null;

            return new IngredientViewModel
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                StockQuantity = ingredient.StockQuantity,
                Unit = ingredient.Unit.ToString()
            };
        }

        public async Task UpdateIngredientAsync(IngredientViewModel model)
        {
            var ingredient = await _context.Ingredients.FindAsync(model.Id);
            if (ingredient == null) throw new Exception("Ingredient not found");

            ingredient.Name = model.Name;
            ingredient.StockQuantity = model.StockQuantity;
            ingredient.Unit = Enum.Parse<Ingredient.IngredientUnit>(model.Unit);

            await _context.SaveChangesAsync();
        }
    }

}
