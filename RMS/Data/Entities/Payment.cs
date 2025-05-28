using RMS.Data.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Data.Entities
{
    public class Payment : BaseEntity
    {
        public int OrderId { get; set; }
        public decimal AmountPaid { get; set; }
        public PaymentMethodEnum PaymentMethod { get; set; }
        [ForeignKey("OrderId")]
        public Order Order { get; set; }
        public enum PaymentMethodEnum
        {
            Cash,
            Card
        }
    }
}
