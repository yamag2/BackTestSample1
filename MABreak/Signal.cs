using BackTest.Lib;
using BackTest.Lib.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackTest.MABreak
{
    public class Signal
    {
        public int Reration { get; set; } // 0:short<long 1:short==long  2:short>long
        public MA MALong { get; set; }
        public MA MAShort { get; set; }
        public SignalEnum Break { get; set; } // 1:GC 2:DC

        public Signal(MA maLong, MA maShort)
        {
            this.MALong = maLong;
            this.MAShort = maShort;
        }

        public void SetUp()
        {
            if (this.MAShort.MAVal < this.MALong.MAVal)
            {
                this.Reration = 0;
            }
            if (this.MAShort.MAVal == this.MALong.MAVal)
            {
                this.Reration = 1;
            }
            if (this.MAShort.MAVal > this.MALong.MAVal)
            {
                this.Reration = 2;
            }
        }

        public void Check()
        {
            var rerationOld = this.Reration;
            SetUp();
            if (this.Reration != rerationOld)
            {
                if (this.Reration > rerationOld && this.Reration == 2)
                {
                    this.Break = SignalEnum.GoldenCloss;// 1:GC 2:DC
                }
                if (this.Reration > rerationOld && this.Reration == 1)
                {
                    this.Break = SignalEnum.Equal;
                }
                if (this.Reration < rerationOld && this.Reration == 1)
                {
                    this.Break = SignalEnum.Equal;
                }
                if (this.Reration < rerationOld && this.Reration == 0)
                {
                    this.Break = SignalEnum.DeadCloss;// 1:GC 2:DC
                }
            }
        }
    }
}
