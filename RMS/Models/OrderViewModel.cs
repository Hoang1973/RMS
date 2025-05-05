using Microsoft.AspNetCore.Mvc.Rendering;
using RMS.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Models
{
    public class OrderViewModel :BaseViewModel
    {
        public int Id { get; set; }
        public string? TableNumber { get; set; }
        public int? CustomerId { get; set; }
        public string? Note { get; set; }
        public string? CustomerPhoneNumber { get; set; }
        [Required(ErrorMessage = "Please select a table")]
        public int TableId { get; set; }
        public IEnumerable<SelectListItem>? AvailableTables { get; set; }
        public decimal? TotalAmount { get; set; }
        public List<DishItem> Dishes { get; set; } = new();
        public class DishItem
        {
            public int DishId { get; set; }
            public string? Name { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }
        public Order.OrderStatus Status { get; set; } = Order.OrderStatus.Pending;
        public double? Discount { get; set; }
    }
}