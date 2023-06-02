using SmartHome.Data;

namespace SmartHome.Tests
{
    public sealed class TestDbContext
    {
        public SmartHomeDbContext CreateContext(bool seed = true)
        {
            var db = new SmartHomeDbContext("DataSource=:memory:");

            if(seed)
                MockDbBuilder.SeedTestDb(db);

            return db;
        }
    }
}
