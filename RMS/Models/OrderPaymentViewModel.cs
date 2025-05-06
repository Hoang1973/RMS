using System.ComponentModel.DataAnnotations;

namespace RMS.Models
{
    public class OrderPaymentViewModel
    {
        public int OrderId { get; set; }
        public int TableId { get; set; }
        public decimal Subtotal { get; set; } // Tổng tiền hàng trước VAT, giảm giá
        public decimal VatPercent { get; set; } = 8; // mặc định 8%
        public int DiscountValue { get; set; } // Giá trị giảm giá (số tiền hoặc %)
        public string DiscountType { get; set; } // "percent" hoặc "amount"
        public string PaymentMethod { get; set; }
        public string TableNumber { get; set; } // (nếu cần)
        public decimal AmountPaid { get; set; } // (nếu cần lưu)
    }
}
