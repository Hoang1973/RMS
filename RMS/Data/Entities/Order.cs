using RMS.Data.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Data.Entities
{
    public class Order : BaseEntity
    {
        public string CustomerPhoneNumber { get; set; }
        public int TableId { get; set; }
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        [ForeignKey("TableId")]
        public Table Table { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public Payment Payment { get; set; }
        public Bill Bill { get; set; }
        public string Note { get; set; }
        public enum OrderStatus
        {
            Pending,
            Processing,
            Ready,
            Completed,
            Cancelled
        }
    }
}
