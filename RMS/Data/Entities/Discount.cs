

using RMS.Data.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Data.Entities
{
    public class Discount : BaseEntity
    {
        public string Name { get; set; }
        public DiscountTypeEnum DiscountType  { get; set; }
        public decimal DiscountValue { get; set; }
        public ICollection<BillDiscount> BillDiscounts { get; set; }

        public enum DiscountTypeEnum
        {
            Percentage,
            Fixed
        }
    }
}
