using System.ComponentModel.DataAnnotations;

namespace BespokeSoftware.Models
{
    public class Login
    {
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public string? role { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        public string UserName { get; set; }
        public int? UserID { get; set; }
    }
}
