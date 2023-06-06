using SmartHome.Data;

namespace SmartHome.Logic
{
    public static class Runtime
    {
        public static SmartHomeDbContext GetContext()
        {
           // string connectionString = Environment.GetEnvironmentVariable("DefaultConnection");

            var dataContext = new SmartHomeDbContext();

            return dataContext;
        }
    }
}
