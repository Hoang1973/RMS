using RMS.Data.Entities.Base;

namespace RMS.Data.Entities
{
    public class Dish : BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<DishIngredient> DishIngredients { get; set; }
    }
}
