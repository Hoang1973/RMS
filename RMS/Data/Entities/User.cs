using RMS.Data.Entities.Base;

namespace RMS.Data.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string Role { get; set; }
    }
}
