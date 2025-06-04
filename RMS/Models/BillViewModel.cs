using System;
using System.Collections.Generic;

namespace RMS.Models
{
    public class BillViewModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal VatPercent { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalDue { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime? CreatedAt { get; set; }

        public List<DiscountViewModel>? Discounts { get; set; } = new();
        public string? TableNumber { get; set; }
        public List<OrderItem>? Items { get; set; } = new();

        public class OrderItem
        {
            public int DishId { get; set; }
            public string Name { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Total { get; set; }
        }
    }
}
