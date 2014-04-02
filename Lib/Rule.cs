using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTest.Lib
{
    public abstract class Rule
    {
        public string[] BuyDays { set; get; }       // Day of Purchasing
        public string[] SellDays { set; get; }      // Day of Selling
        public double DrawDownRate { set; get; }    // Rate of DrawDown
        public double BuyOutRate { set; get; }      // Rate to BuyOut
        public int GrabDayCount { set; get; }       // Day Count to grab position
    }
}
