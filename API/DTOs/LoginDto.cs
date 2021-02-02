using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    // DTOs are a good place to validate since it has all of the necessary inputs and nothing extra.
    public class LoginDto
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9-\.]+$",
            ErrorMessage = "Usernames can only have alphanumeric characters, '-', and '.'")]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}