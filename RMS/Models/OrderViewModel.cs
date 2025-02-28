using RMS.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Models
{
    public class OrderViewModel :BaseViewModel
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }

        [Required(ErrorMessage = "Table selection is required")]
        public int TableId { get; set; }
        public decimal TotalAmount => Dishes.Sum(d => d.Price * d.Quantity);
        public Order.OrderStatus Status { get; set; }
        public List<DishItem> Dishes { get; set; } = new();
        public class DishItem
        {
            public int DishId { get; set; }
            public string? Name { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
        }
        //public TableItem Table { get; set; } = new();
        //public ICollection<TableItem> AvailableTables { get; set; } = new List<TableItem>();
        //public class TableItem
        //{
        //    public int TableId { get; set; }
        //    public string? TableNumber { get; set; }
        //    public int Capacity { get; set; }
        //    public Table.TableStatus Status { get; set; }
        //}
    }
}