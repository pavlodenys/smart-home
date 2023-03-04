using SmartHome.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Tests
{
    public static class MockDbBuilder
    {
        public static void SeedTestDb(SmartHomeDbContext dbContext)
        {
            dbContext.Database.EnsureCreated();

        }
    }
}
