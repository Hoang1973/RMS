using RMS.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace RMS.Models
{
    public class DishViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity must be zero or positive.")]
        public decimal Price { get; set; }
        public string Description { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        public Dish.DishType Type { get; set; }
    }
}
