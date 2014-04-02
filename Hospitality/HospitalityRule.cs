using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BackTest.Lib;

namespace BackTest.Hospitality
{
    public class HospitalityRule : Rule
    {
        public HospitalityRule()
        {
            this.DrawDownRate = 0.15;
        }
    }
}
