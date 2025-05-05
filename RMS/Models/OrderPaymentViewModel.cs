using System.ComponentModel.DataAnnotations;

namespace RMS.Models
{
    public class OrderPaymentViewModel
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public decimal Subtotal { get; set; } // = Order.TotalAmount (chưa VAT, giảm giá)
        public decimal VatPercent { get; set; } = 8; // mặc định 8%
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; } // Đã tính VAT, giảm giá
        public decimal TotalDue { get; set; }    // = TotalAmount
        [Required]
        public decimal AmountPaid { get; set; }
        [Required]
        public string PaymentMethod { get; set; }
    }
}
