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
        public static Context GetContext()
        {
            var dataContext = new Context();

            return dataContext;
        }
    }
}
