using System.ComponentModel.DataAnnotations;
using RMS.Data.Entities;

namespace RMS.Models
{
    public class PaymentViewModel
    {
        public int OrderId { get; set; }
        public string TableNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderViewModel.DishItem> Dishes { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Vat { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        [Required]
        public decimal AmountPaid { get; set; }
        [Required]
        public Payment.PaymentMethodEnum PaymentMethod { get; set; }
    }
}
