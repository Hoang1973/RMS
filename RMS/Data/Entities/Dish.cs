using RMS.Data.Entities.Base;

namespace RMS.Data.Entities
{
    public class Dish : BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public DishType Type { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<DishIngredient> DishDishes { get; set; }
        public enum DishType
        {
            MainCourse,
            Appetizer,
            Dessert
        }
    }
}
