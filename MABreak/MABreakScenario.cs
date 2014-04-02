using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTest.Lib;
using System.Collections;
using BackTest.Lib.Base;
using System.Net;
using System.IO;
using Newtonsoft.Json.Linq;
using BackTest.Lib.Const;

namespace BackTest.MABreak
{
    public class MABreakScenario : ScenarioBase
    {
        const int DATE = 0; // YYYYMMDD
        const int START = 1; // Value of START
        const int END = 4; // Value of End
        const int BUYDAY = 0; // 
        const int SELLDAY = 1; // VerifyResults
        protected Signal signal;

        protected override Rule SetupRules()
        {
            MABreakRule rule = new MABreakRule();
            rule.BuyDays = new string[] { "0401", "0402", "0403", "0404", };            //Day of Purchasing
            rule.SellDays = new string[] { "0626", "0627", "0628", "0629", "0630", };   //Day of Selling
            rule.GrabDayCount = 60;
            return rule;
        }

        protected override Configuration ReadConfigurations()
        {
            string sDirRoot = System.Configuration.ConfigurationManager.AppSettings["Dir"];
            string sRptOut = System.Configuration.ConfigurationManager.AppSettings["ReportOutDir"];
            return new MABreakConfig() { DocRoot = sDirRoot, ReportOut = sRptOut };
        }

        protected override Corp SetupCorp(object baseData)
        {
            var corpData = (DictionaryEntry)baseData;
            MABreakCorp retCorp = new MABreakCorp
            {
                CorpValList = (ArrayList)corpData.Value,
                CorpNo = (string)((ArrayList)corpData.Value).Cast<ArrayList>().FirstOrDefault()[0],
                MA75 = new MA(75),
                MA25 = new MA(25),
            };
            retCorp.Signal = new Signal(retCorp.MA25, retCorp.MA75);
            return retCorp;
        }

        protected override void AnalizeFootData(Rule rule, ArrayList footData, CorpBase corpBase)
        {
            if (((string)footData[DATE]).Length < 8) { return; }
            try
            {
                var corp = (MABreakCorp)corpBase;
                // Constructing MA.
                corp.MA75.SetUp(Convert.ToInt32((string)footData[END]), (string)footData[DATE]);
                corp.MA25.SetUp(Convert.ToInt32((string)footData[END]), (string)footData[DATE]);
                if (corp.MA25.Ready && corp.MA75.Ready)
                {
                    if (corp.MA75.Changed)
                    {
                        corp.Signal.SetUp();
                    }
                    else
                    {
                        corp.Signal.Check();
                    }

                    // TO BUY
                    if (corp.Signal.Break == SignalEnum.GoldenCloss)
                    {
                        corp.BullPrice = Convert.ToInt64((string)footData[END]);
                        corp.BullAmount = 1;
                        corp.BullDate = MyUtil.ConvertToDatetime((string)footData[DATE]);
                        corp.HasPosition = true;
                        corp.PosDate = (string)footData[DATE];
                        return;
                    }

                    // TO SELL
                    int iValNow = Convert.ToInt32((string)footData[START]);
                    double iThdDown = corp.BullPrice * (1 - rule.DrawDownRate);
                    double iThdUp = corp.BullPrice * rule.BuyOutRate;
                    if (corp.HasPosition == true)
                    {
                        // Judge a Term Grabbing the Position
                        var dtJudgeStr = Convert.ToInt32(MyUtil.GetYYYYMMDD(MyUtil.ConvertToDatetime(corp.PosDate, rule.GrabDayCount)));
                        if (dtJudgeStr <= Convert.ToInt32((string)footData[DATE]))
                        {
                            SellPosition(MyUtil.ConvertToDatetime((string)footData[DATE]), corp, footData);
                            corp.HasPosition = false;
                        }
                        if (iValNow <= iThdDown || iValNow >= iThdUp)
                        {
                            SellPosition(MyUtil.ConvertToDatetime((string)footData[DATE]), corp, footData);
                            corp.HasPosition = false;
                        }
                    }
                }
                //((MABreakCorp)corpBase).MA75 = lgArr75;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadLine();
            }
        }

        protected override VerifiedResults VerifyResults(AnalizedResults analRslt)
        {
            long iCalc = 0;
            var dicRslt = new Dictionary<string, object>();
            foreach (CorpBase corp in analRslt.CorpList)
            {
                int rsltCnt = corp.TradeValList.Count;
                foreach (string[] trade in corp.TradeValList)
                {
                    iCalc += (Convert.ToInt32(trade[3]) - Convert.ToInt32(trade[2]));
                }
                dicRslt.Add(corp.CorpNo, new string[] { iCalc.ToString() });
                iCalc = 0;
            }
            return new VerifiedResultsBase() { VerifiedResultArray = dicRslt, CorpList = analRslt.CorpList };
        }

        private void SellPosition(DateTime sDate, CorpBase corpBase, ArrayList footData)
        {
            string sBullDt = MyUtil.GetYYYYMMDD(corpBase.BullDate);
            ArrayList valList = corpBase.TradeValList;
            valList.Add(new string[]{ 
	                                sBullDt,                     // DATE TO BUY 
	                                (string)footData[DATE],      // DATE TO SELL 
	                                corpBase.BullPrice.ToString(),// PRICE TO BUY 
	                                (string)footData[START],     // PRICE TO SELL
	                                corpBase.AmountUnit.ToString(),// AMOUNT UNIT TO BUY
	                                (Convert.ToInt64((string)footData[START]) - corpBase.BullPrice).ToString() // Barance
	                            });
            corpBase.TradeValList = valList;
            corpBase.BairAmount = 1;
            corpBase.BairDate = sDate;
            corpBase.BairPrice = Convert.ToInt64((string)footData[START]);
            Console.WriteLine(corpBase.CorpNo + "[BUY]:" + sBullDt + "@" + corpBase.BullPrice);
            var iGrbDays = (MyUtil.ConvertToDatetime((string)footData[DATE]) - MyUtil.ConvertToDatetime(sBullDt)).Days;
            Console.WriteLine(corpBase.CorpNo + "[SELL]:" + sDate + "@" + corpBase.BairPrice + "<Grabs:" + (iGrbDays) + ">");
            corpBase.BullPrice = 0;
            corpBase.BullDate = DateTime.MinValue;
        }


        protected override Report CreateReports(VerifiedResults vrfRslt)
        {
            var vrfdRsltArr = ((VerifiedResultsBase)vrfRslt).VerifiedResultArray;
            foreach (KeyValuePair<string, object> data in vrfdRsltArr)
            {
                string[] sRsltArr = (string[])data.Value;
                if (Convert.ToInt64(sRsltArr[0]) > 0)
                {
                    Console.WriteLine(data.Key + ":" + sRsltArr[0]);
                }
            }

            #region all trade history csv out
            using (StreamWriter sw = new StreamWriter(vrfRslt.Config.ReportOut, false, System.Text.Encoding.GetEncoding("shift_jis")))
            {
                long lgSum = 0;
                sw.WriteLine(string.Join("\t", new string[] { "BuyD", "SellD", "BuyV", "SellV", "Unit", "Total", }));
                foreach (var corp in ((VerifiedResultsBase)vrfRslt).CorpList)
                {
                    sw.WriteLine(string.Join("\t", corp.CorpNo));
                    foreach (string[] valData in ((CorpBase)corp).TradeValList)
                    {
                        sw.WriteLine(string.Join("\t", valData));
                        lgSum += (Convert.ToInt64(valData[3]) - Convert.ToInt64(valData[2]));
                    }
                    sw.WriteLine(string.Join("\t", new string[] { "AccmTotal", "", "", "", "", lgSum.ToString(), }));
                    lgSum = 0;
                }
                sw.Close();
            }

            #endregion

            Console.ReadLine();
            return null;
        }
    }
}
