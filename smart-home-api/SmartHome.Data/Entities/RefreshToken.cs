using System.ComponentModel.DataAnnotations.Schema;

namespace SmartHome.Data.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        [ForeignKey(nameof(UserId))]
        public virtual HomeUser User { get; set; } = new HomeUser();
        public bool IsExpired { get; set; }
    }
}