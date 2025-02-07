using System.ComponentModel.DataAnnotations;

namespace RMS.Models
{
    public class IngredientViewModel
    {
        public int Id { get; set; }  // Only needed for updates

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock Quantity must be zero or positive.")]
        public int StockQuantity { get; set; }

        [Required(ErrorMessage = "Unit is required.")]
        public string Unit { get; set; } // Use string instead of enum to simplify dropdowns in the UI

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
    }
}
