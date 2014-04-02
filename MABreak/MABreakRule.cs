using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BackTest.Lib;

namespace BackTest.MABreak
{
    public class MABreakRule : Rule
    {
        public int ContinualTendencyCount { get; set; }
        public MABreakRule()
        {
            this.DrawDownRate = 0.15;
            this.BuyOutRate = 1.15;
        }
    }
}
