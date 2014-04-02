using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BackTest.Lib.IF;

namespace BackTest.Lib
{
    public class MA : IMA
    {
        private int iCircuit = 25;
        private List<long> maHisList = new List<long>();
        private List<long> maDateList = new List<long>();

        public List<long> MaHisList
        {
            get { return this.maHisList; }
            set { this.maHisList = value; }
        }
        public List<long> MaDateList
        {
            get { return this.maDateList; }
            set { this.maDateList = value; }
        }
        public long MAVal { get; set; }
        public bool Changed { get; set; }
        public bool Ready { get; set; }

        public MA(int iParam)
        {
            this.iCircuit = iParam;
            this.maHisList = new List<long>();
        }

        public void SetUp(int iVal, string sDate)
        {
            if (this.maHisList.Count() < this.iCircuit)
            {
                this.maHisList.Add(iVal);
                this.maDateList.Add(Convert.ToInt32(sDate));
                return;
            }
            if (this.maHisList.Count() >= this.iCircuit)
            {
                long lgSum = 0;
                if (this.maHisList.Count() == this.iCircuit)
                {
                    this.maHisList.RemoveAt(0);
                    this.maHisList.Add(iVal);
                    this.maDateList.RemoveAt(0);
                    this.maDateList.Add(Convert.ToInt32(sDate));
                    if (this.Ready == false)
                    {
                        this.Changed = true;
                    }
                    else
                    {
                        this.Changed = false;
                    }
                    this.Ready = true;
                }
                for (int i = 0; i < this.maHisList.Count(); i++)
                {
                    lgSum += this.maHisList.ToArray()[i];
                }
                this.MAVal = lgSum / this.iCircuit;
            }
        }
    }
}
