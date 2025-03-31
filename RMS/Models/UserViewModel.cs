using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using RMS.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace RMS.Models
{
    public class UserViewModel : BaseViewModel
    {
        public int Id { get; set; }
        [StringLength(50)]
        public string? Name { get; set; }
        [StringLength(50)]
        public string? Email { get; set; }
        [StringLength(50)]
        public string? Password { get; set; }
        [StringLength(11)]
        public string? Phone { get; set; }
        [StringLength(100)]
        public string? Address { get; set; }
        public User.Role UserRole { get; set; }

    }
}
