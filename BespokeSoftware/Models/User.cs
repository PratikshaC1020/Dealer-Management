using System.ComponentModel.DataAnnotations;

namespace BespokeSoftware.Models
{
    public class User
    {
        public int UserID { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(150)]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Format")]
        [StringLength(150)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mobile No is required")]
        [RegularExpression(@"^[0-9]{10,15}$", ErrorMessage = "Enter valid mobile number")]
        public string MobileNo { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*\d).{4,}$", ErrorMessage = "Password must contain minimum 4 characters and one number")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Please select Department")]
        public int? DepID { get; set; }

        [Required(ErrorMessage = "Please select Role")]
        public int? RoleId { get; set; }
        public string? Department { get; set; }
        public string? Role { get; set; }

        public bool IsActive { get; set; }

        public List<User> UserList { get; set; } = new List<User>();
    }
}