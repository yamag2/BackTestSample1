using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace BackTest.Lib.Base
{
    public class CorpBase : Corp
    {
        // A CORP TRADE HISTORY([0]:DATE TO BUY [1]:DATE TO SELL [2]:PRICE TO BUY [3]:PRICE TO SELL [4]:AMOUNT UNIT TO BUY)
        private ArrayList tradeValList;

        public ArrayList CorpValList { get; set; }
        public ArrayList TradeValList
        {
            get
            {
                if (tradeValList == null)
                {
                    return new ArrayList();
                }
                return this.tradeValList;
            }
            set { this.tradeValList = value; }
        }
    }
}
