using System.ComponentModel.DataAnnotations;

namespace SmartHome.Data.DTO
{
    public class LoginDto
    {
        //public string Email { get; set; }
        [Required]
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}