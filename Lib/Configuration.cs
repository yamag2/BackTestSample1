using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTest.Lib
{
    public abstract class Configuration
    {
        public string DocRoot { get; set; }
        public string ReportOut { get; set; }
    }
}
