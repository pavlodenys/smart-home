using SmartHome.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
