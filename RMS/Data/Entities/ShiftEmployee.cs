using System.ComponentModel.DataAnnotations.Schema;
using RMS.Data.Entities.Base;
namespace RMS.Data.Entities
{
    public class ShiftEmployee : BaseEntity
    {
        public int EmployeeId { get; set; }
        public DateTime ShiftDate { get; set; }
        public int ShiftId { get; set; } // FK to Shift
        public ShiftStatus Status { get; set; } = ShiftStatus.Pending;

        [ForeignKey("EmployeeId")]
        public User Employee { get; set; }

        [ForeignKey("ShiftId")]
        public Shift Shift{ get; set; }
        public enum ShiftStatus
        {
            Pending,
            Approved,
            Rejected,
            Assigned   // System automatically assigned shift
        }
    }
}

