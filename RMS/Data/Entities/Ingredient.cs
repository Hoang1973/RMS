using RMS.Data.Entities.Base;

namespace RMS.Data.Entities
{
    public class Ingredient : BaseEntity
    {
        public string Name { get; set; }
        public int StockQuantity { get; set; }
        public IngredientUnit Unit { get; set; }
        public IngredientType Type { get; set; }
        public ICollection<DishIngredient> DishIngredients { get; set; }
        public ICollection<Stock> Stocks { get; set; }
        public enum IngredientUnit
        {
            kg,
            liters,
            pieces
        }
        public enum IngredientType
        {
            Meat,
            Vegetable,
            Fruit,
            Dairy,
            Grain,
            Spice,
            Sweetener,
            Fat,
            Other
        }
    }
}
