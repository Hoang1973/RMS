using RMS.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace RMS.Models
{
    public class OrderViewModel :BaseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Table is required")]
        public int TableId { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be positive")]
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Order status is required")]
        public Order.OrderStatus Status { get; set; }
        public TableItem Table { get; set; } = new();
        public class TableItem
        {
            public int TableId { get; set; }
            public string? TableNumber { get; set; }
            public int Capacity { get; set; }
            public Table.TableStatus Status { get; set; }
        }
    }
}
