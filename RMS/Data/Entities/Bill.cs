using RMS.Data.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Data.Entities
{
    public class Bill : BaseEntity
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TotalDue { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }
        public ICollection<BillDiscount> BillDiscounts { get; set; }
    }
}
