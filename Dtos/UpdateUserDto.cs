using System.ComponentModel.DataAnnotations;

namespace Backend.Dtos
{
    public class UpdateUserDto
    {
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
        public string? Password { get; set; }

        [MaxLength(30, ErrorMessage = "User name cannot exceed 50 characters")]
        public string? UserName { get; set; }
        public string? City { get; set; }
        public string? Fav_Food { get; set; }
    }
}
