using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartHome.Data.Entities;

namespace SmartHome.Data
{
    public class SmartHomeDbContext : DbContext
    {
        // private readonly string _connectionString = "Server=.;Database=SmartHouse;Integrated Security=true;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True";

        private readonly string _connectionString = "Server=192.168.3.21,1433;Database=SmartHouse;User Id=dockerASP;Password=Rabbitball23;TrustServerCertificate=True";

        public SmartHomeDbContext() => new SmartHomeDbContext("Server=192.168.3.21,1433;Database=SmartHouse;User Id=dockerASP;Password=Rabbitball23;TrustServerCertificate=True");

        public SmartHomeDbContext(string connectionString) {
            _connectionString = connectionString;
        }
        //public Context(DbContextOptions<Context> options):base(options) { }

        public DbSet<Sensor> Sensors { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<Scenario> Scenarios { get; set; }
        public DbSet<ScenarioDevice> ScenarioDevices { get; set; }
        public DbSet<ScenarioSensor> ScenarioSensors { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<HomeUser> HomeUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_connectionString);
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Sensor>()
        //        .HasOne(s => s.Data)
        //        .WithMany()
        //        .HasForeignKey(s => s.DataId);
        //}   
    }
}
