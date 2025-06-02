using RMS.Data.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Data.Entities
{
    public class OrderItem : BaseEntity
    {
        public int OrderId { get; set; }
        public int DishId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }
        [ForeignKey("DishId")]
        public Dish Dish { get; set; }
        public bool IsCompleted { get; set; }
    }
}
