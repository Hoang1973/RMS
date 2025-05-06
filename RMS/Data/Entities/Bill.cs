using RMS.Data.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Data.Entities
{
    public class Bill : BaseEntity
    {
        public int OrderId { get; set; }
        public decimal Subtotal { get; set; } // Tổng tiền hàng chưa VAT, chưa giảm giá
        public decimal VatPercent { get; set; } // Phần trăm VAT
        public decimal VatAmount { get; set; } // Số tiền VAT
        public decimal TotalAmount { get; set; } // Tổng sau VAT, trước giảm giá
        public decimal DiscountAmount { get; set; } // Số tiền giảm giá
        public decimal TotalDue { get; set; } // Thành tiền cuối cùng
        [ForeignKey("OrderId")]
        public Order Order { get; set; }
        public ICollection<BillDiscount> BillDiscounts { get; set; }
    }
}
