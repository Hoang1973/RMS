using Microsoft.AspNetCore.Mvc.Rendering;
using RMS.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace RMS.Models
{
    public class DishViewModel : BaseViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity must be zero or positive.")]
        public decimal Price { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        public Dish.DishType Type { get; set; }
        public List<IngredientItem> Ingredients { get; set; } = new();
        public class IngredientItem
        {
            public int IngredientId { get; set; }
            public string IngredientName { get; set; } 
            public decimal QuantityNeeded { get; set; }
            public Ingredient.IngredientType IngredientType { get; set; }
            public Ingredient.IngredientUnit IngredientUnit { get; set; }
        }

        public IEnumerable<SelectListItem> TypeOptions => Enum.GetValues(typeof(Dish.DishType))
            .Cast<Dish.DishType>()
            .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString() })
            .ToList();
    }
}
