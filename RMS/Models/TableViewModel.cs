using RMS.Data.Entities;
using System.ComponentModel.DataAnnotations;
namespace RMS.Models
{
    public class TableViewModel : BaseViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Table number is required")]
        public string? TableNumber { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }
        public Table.TableStatus Status = Table.TableStatus.Available; // Default status

        //public string StatusDisplay => Status.ToString(); // Readable status display

        //public ICollection<OrderViewModel> Orders { get; set; } = new List<OrderViewModel>(); // Assuming an OrderViewModel exists
    }
}
