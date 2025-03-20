using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email is Requierd")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is Requierd")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "FirstName is Requierd")]
        [MaxLength(30, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "LastName is Requierd")]
        [MaxLength(30, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "UserName is Requierd")]
        [MaxLength(30, ErrorMessage = "User name cannot exceed 50 characters")]
        public string UserName { get; set; }



    }
}
