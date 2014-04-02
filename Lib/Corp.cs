using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackTest.Lib
{
    public abstract class Corp
    {
        public string CorpNo { get; set; }
        public Int32 AmountUnit { get; set; }
        public Int32 BullAmount { get; set; }   // Amount of Buy Position
        public Int32 BairAmount { get; set; }   // Amount of Sell Position
        public DateTime BullDate { get; set; }    // Date of Buy Position
        public DateTime BairDate { get; set; }    // Date of Sell Position
        public Int64 BullPrice { get; set; }    // Price of Buy Position
        public Int64 BairPrice { get; set; }    // Price of Sell Position
        public bool HasPosition { get; set; }   // weather has Position or Not
    }
}
