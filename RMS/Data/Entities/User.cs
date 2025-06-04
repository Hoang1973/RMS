using RMS.Data.Entities.Base;

namespace RMS.Data.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string Phone { get; set; }
        public string? Address { get; set; }
        public Role UserRole { get; set; }
        public ICollection<Shift> Shifts { get; set; }
        public enum Role
        {
            Admin = 1,
            User = 2,
            Chef = 3,
            Waiter = 4,
            Cashier = 5
        }

    }
}
