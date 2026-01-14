using System.ComponentModel.DataAnnotations;

namespace iTaxSuite.Library.Models.ViewModels
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "User Name is required")]
        [Display(Name = "Phone Number")]
        public string? Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "First Name is required")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is required")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Please Enter Phone Number")]
        [Display(Name = "Phone Number")]
        [StringLength(13, ErrorMessage = "The Mobile must contains 10 characters", MinimumLength = 13)]
        public string PhoneNumber { get; set; }
    }

    public class LoginModel
    {
        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }
    }

    public class LoginResponse
    {
        public string? UserName { get; set; }
        public string? Token { get; set; }
        public int ExpiresIn { get; set; }
    }

}
