using RMS.Data.Entities.Base;

namespace RMS.Data.Entities
{
    public class Table : BaseEntity
    {
        public string TableNumber { get; set; }
        public int Capacity { get; set; }
        public TableStatus Status { get; set; }

        public ICollection<Order> Orders { get; set; }
        public enum TableStatus
        {
            Available = 1,
            Occupied = 2,
        }
    }
}
