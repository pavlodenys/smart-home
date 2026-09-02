using Microsoft.EntityFrameworkCore;
using SmartHome.Data.Entities;

namespace SmartHome.Data
{
    public class SmartHomeDbContext : DbContext
    {
        private readonly string _connectionString;

        public SmartHomeDbContext()
        {
            _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings__DefaultConnection is required.");
        }

        public SmartHomeDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public SmartHomeDbContext(DbContextOptions options) : base(options)
        {
            _connectionString = string.Empty;
        }

        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Scenario> Scenarios { get; set; }
        public DbSet<ScenarioDevice> ScenarioDevices { get; set; }
        public DbSet<ScenarioSensor> ScenarioSensors { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<HomeUser> HomeUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_connectionString);
            }
        }
    }
}
