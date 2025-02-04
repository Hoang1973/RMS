using RMS.Data.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Data.Entities
{
    public class Discount : BaseEntity
    {
        public string Name { get; set; }
        public string DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public ICollection<BillDiscount> BillDiscounts { get; set; }
        [ForeignKey("IngredientId")]
        public Ingredient Ingredient { get; set; }
    }
}
