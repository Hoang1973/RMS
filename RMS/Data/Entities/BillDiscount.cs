using RMS.Data.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Data.Entities
{
    public class BillDiscount : BaseEntity
    {
        public int BillId { get; set; }
        public int DiscountId { get; set; }
        [ForeignKey("BillId")]
        public Bill Bill { get; set; }
        [ForeignKey("DiscountId")]
        public Discount Discount { get; set; }
    }
}
