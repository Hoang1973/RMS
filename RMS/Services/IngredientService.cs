using RMS.Data.Entities;
using RMS.Data;
using RMS.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace RMS.Services
{
    public interface IIngredientService
    {
        Task<List<IngredientViewModel>> GetAllIngredientsAsync();
        Task<IngredientViewModel?> GetIngredientByIdAsync(int id);
        Task CreateIngredientAsync(IngredientViewModel model);
        Task UpdateIngredientAsync(IngredientViewModel model);
        Task<bool> DeleteIngredientByIdAsync(int id);
        Task<bool> IngredientExistsAsync(int id);
    }
    public class IngredientService : IIngredientService
    {
        private readonly RMSDbContext _context;
        private readonly IMapper _mapper;

        public IngredientService(RMSDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<IngredientViewModel>> GetAllIngredientsAsync()
        {
            var ingredients = await _context.Ingredients.ToListAsync();
            return _mapper.Map<List<IngredientViewModel>>(ingredients);
        }

        public async Task<IngredientViewModel?> GetIngredientByIdAsync(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            return ingredient == null ? null : _mapper.Map<IngredientViewModel>(ingredient);
        }

        public async Task CreateIngredientAsync(IngredientViewModel model)
        {
            var ingredient = _mapper.Map<Ingredient>(model);
            ingredient.Unit = Enum.Parse<Ingredient.IngredientUnit>(model.Unit);
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateIngredientAsync(IngredientViewModel model)
        {
            var ingredient = await _context.Ingredients.FindAsync(model.Id);
            Console.WriteLine($"Before Save: {model.CreatedAt}");
            if (ingredient == null) throw new Exception("Ingredient not found");

            _mapper.Map(model, ingredient);

            ingredient.Unit = Enum.Parse<Ingredient.IngredientUnit>(model.Unit);

            await _context.SaveChangesAsync();
        }
        public async Task<bool> DeleteIngredientByIdAsync(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null) return false;

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> IngredientExistsAsync(int id)
        {
            return await _context.Ingredients.AnyAsync(e => e.Id == id);
        }
    }

}
