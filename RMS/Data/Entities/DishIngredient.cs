using RMS.Data.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Data.Entities
{
    public class DishIngredient : BaseEntity
    {
        public int DishId { get; set; }
        public int IngredientId { get; set; }
        public decimal QuantityNeeded { get; set; }
        [ForeignKey("DishId")]
        public Dish Dish { get; set; }
        [ForeignKey("IngredientId")]
        public Ingredient Ingredient { get; set; }
    }
}
