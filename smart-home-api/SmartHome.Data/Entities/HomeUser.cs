using Microsoft.AspNetCore.Identity;

namespace SmartHome.Data.Entities
{
    public class HomeUser : IdentityUser<int>
    {
        public override int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public override string? Email { get; set; } = string.Empty;
        // public string PasswordHash { get; set; }
        public override string? UserName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public virtual List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    }
}