using System.ComponentModel.DataAnnotations;
namespace RMS.Models
{
    public class LoginModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        public string SessionId { get; set; }
    }
}
