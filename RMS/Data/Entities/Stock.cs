using RMS.Data.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Data.Entities
{
    public class Stock : BaseEntity
    {
        public int IngredientId { get; set; }
        public int StockChange { get; set; }
        public DateTime StockDate { get; set; }
        [ForeignKey("IngredientId")]
        public Ingredient Ingredient { get; set; }
    }
}
