using Microsoft.EntityFrameworkCore;
using SmartHome.Data.Entities;

namespace SmartHome.Data
{
    public class SmartHomeDbContext : DbContext
    {
        // private readonly string _connectionString = "Server=.;Database=SmartHouse;Integrated Security=true;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=True";

        private readonly string _connectionString;

        public SmartHomeDbContext()
        {
            _connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Port=5432;Database=SmartHouse;Username=smarthome;Password=SmartHome_local_2026!";
        }

        public SmartHomeDbContext(string connectionString) {
            _connectionString = connectionString;
        }

        public SmartHomeDbContext(DbContextOptions options) : base(options)
        {
            _connectionString = string.Empty;
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
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(_connectionString);
            }
        }

        //public override EntityEntry Remove(object entity)
        //{
        //    if(entity.GetType().BaseType!= null && entity.GetType().BaseType.Name == "IDeleted")
        //    {
        //        ((IDeleted)entity).IsDeleted = true;

        //        return new EntityEntry();
        //    } else
        //    {
        //        return base.Remove(entity);
        //    }
        //}

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<Sensor>()
        //        .HasOne(s => s.Data)
        //        .WithMany()
        //        .HasForeignKey(s => s.DataId);
        //}   
    }
}
