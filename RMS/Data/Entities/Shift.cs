using RMS.Data.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Data.Entities
{
    public class Shift : BaseEntity
    {
        public int EmployeeId { get; set; }
        public DateTime ShiftStart { get; set; }
        public DateTime ShiftEnd { get; set; }
        [ForeignKey("EmployeeId")]
        public User Employee { get; set; }
    }
}
