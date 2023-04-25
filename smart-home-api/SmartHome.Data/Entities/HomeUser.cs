using Microsoft.AspNetCore.Identity;

namespace SmartHome.Data.Entities
{
    public class HomeUser : IdentityUser<int>
    {
        public override int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public override string? Email { get; set; }
        // public string PasswordHash { get; set; }
        public override string? UserName { get; set; }
        public bool IsActive { get; set; }
        public virtual List<RefreshToken> RefreshTokens { get; set; }

    }
}