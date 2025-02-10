using RMS.Data.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace RMS.Data.Entities
{
    public class Shift : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } // Morning, Evening, Night
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}
