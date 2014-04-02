using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BackTest.Lib.Base;
using BackTest.Lib;
using BackTest.MABreak;

namespace BackTest.MABreak
{
    public class MABreakCorp : CorpBase
    {
        public MABreakCorp()
            : base()
        {
        }

        public MA MA25 { get; set; }
        public MA MA75 { get; set; }
        public Signal Signal { get; set; }
        public string PosDate { get; set; }
    }
}
