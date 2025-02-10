using Microsoft.AspNetCore.Mvc.Rendering;
using RMS.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace RMS.Models
{
    public class IngredientViewModel : BaseViewModel
    {
        public int Id { get; set; }  // Only needed for updates

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity must be zero or positive.")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Unit is required.")]
        public Ingredient.IngredientUnit Unit { get; set; }
        [Required(ErrorMessage = "Type is required.")]
        public Ingredient.IngredientType Type { get; set; }

        public IEnumerable<SelectListItem> UnitOptions => Enum.GetValues(typeof(Ingredient.IngredientUnit))
            .Cast<Ingredient.IngredientUnit>()
            .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString() })
            .ToList();
        public IEnumerable<SelectListItem> TypeOptions => Enum.GetValues(typeof(Ingredient.IngredientType))
            .Cast<Ingredient.IngredientType>()
            .Select(e => new SelectListItem { Value = e.ToString(), Text = e.ToString() })
            .ToList();
    }
}
