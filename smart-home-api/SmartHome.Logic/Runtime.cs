using SmartHome.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartHome.Logic
{
    public static class Runtime
    {
        public static SmartHomeDbContext GetContext()
        {
            var dataContext = new SmartHomeDbContext();

            return dataContext;
        }
    }
}
