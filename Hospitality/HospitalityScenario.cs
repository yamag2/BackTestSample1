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

namespace BackTest.Hospitality
{
    public class HospitalityScenario : ScenarioBase
    {
        const int DATE = 0; // YYYYMMDD
        const int START = 1; // Value of START
        const int END = 4; // Value of End
        const int BUYDAY = 0; // 
        const int SELLDAY = 1; // VerifyResults

        protected override Rule SetupRules()
        {
            HospitalityRule rule = new HospitalityRule();
            rule.BuyDays = new string[] { "0401", "0402", "0403", "0404", };            //Day of Purchasing
            rule.BuyDays = new string[] { "0413", "0414", "0415", "0416", };            //Day of Purchasing
            rule.BuyDays = new string[] { "0420", "0421", "0422", "0423", };            //Day of Purchasing
            rule.BuyDays = new string[] { "0427", "0428", "0429", "0430", };            //Day of Purchasing
            rule.BuyDays = new string[] { "0501", "0502", "0503", "0504", "0505", "0506", "0507", };            //Day of Purchasing
            rule.BuyDays = new string[] { "0407", "0408", "0409", "0410", };            //Day of Purchasing

            rule.SellDays = new string[] { "0626", "0627", "0628", "0629", "0630", };   //Day of Selling

            return rule;
        }

        protected override Configuration ReadConfigurations()
        {
            string sDirRoot = System.Configuration.ConfigurationManager.AppSettings["Dir"];
            string sRptOut = System.Configuration.ConfigurationManager.AppSettings["ReportOutDir"];
            return new HospitalityConfig() { DocRoot = sDirRoot, ReportOut = sRptOut };
        }

        protected override StaticData GetStaticData()
        {
            StaticDataBase retStData = new StaticDataBase();
            Hashtable hshCrpSt = new Hashtable();
            #region  WebClientTest
            WebClient wc = new WebClient();

            Stream st = wc.OpenRead("http://free-ec2.scraperwiki.com/cnnyrcy/5f3e5381aeec444/sql/?q=select%20%0A%09id%2C%0A%09yyyymm%2C%0A%09code%2C%0A%20%20%20%20amount_unit%0Afrom%20swdata%0A%20where%20yyyymm%3D%27201407%27%0Aorder%20by%20yyyymm%0A--limit%20100");
            Encoding enc = Encoding.GetEncoding("Shift_JIS");
            StreamReader sr = new StreamReader(st, enc);
            string html = sr.ReadToEnd();
            sr.Close();
            st.Close();

            #region JSON-GET
            html = html.Replace("[", "").Replace("]", "").Replace("},", "}@");
            string[] sArr = html.Split('@');
            foreach (var item in sArr)
            {
                JObject jsnRslt = JObject.Parse(item);
                hshCrpSt.Add(jsnRslt["code"].ToString(), jsnRslt["amount_unit"].ToString());
            }
            retStData.CorpStaticData = hshCrpSt;
            #endregion

            #endregion
            return retStData;
        }

        protected override void AnalizeFootData(Rule rule, ArrayList footData, CorpBase corpBase)
        {
            string sDate = (string)footData[DATE];
            string sBullDt = MyUtil.GetYYYYMMDD(corpBase.BullDate);
            try
            {
                // DATE MATCHING - BUY
                foreach (var buyDay in rule.BuyDays)
                {
                    if (sDate.Substring(sDate.Length - 4).Equals(buyDay) && corpBase.HasPosition == false)
                    {
                        corpBase.BullPrice = Convert.ToInt64((string)footData[END]);
                        corpBase.BullAmount = 1;
                        corpBase.BullDate = MyUtil.ConvertToDatetime(sDate);
                        corpBase.HasPosition = true;
                        break;
                    }
                }

                // MANAGE DRAW DOWN SITUATION
                if (sDate.Length > 4 && corpBase.BullPrice > 0)
                {
                    int iValNow = Convert.ToInt32((string)footData[START]);
                    double iThreshold = corpBase.BullPrice * (1 - rule.DrawDownRate);
                    if (iValNow <= iThreshold)
                    {
                        SellPosition(MyUtil.ConvertToDatetime(sDate), corpBase, footData);
                        corpBase.HasPosition = false;
                    }
                    //return;
                }

                DateTime dtSell = DateTime.MinValue;
                // DATE MATCHING - SELL
                foreach (var SellDay in rule.SellDays)
                {
                    if (sDate.Substring(sDate.Length - 4).Equals(SellDay) && corpBase.HasPosition == true)
                    {
                        dtSell = MyUtil.ConvertToDatetime((string)footData[DATE]);
                        if (dtSell.CompareTo(corpBase.BullDate) > 0 && !corpBase.TradeValList.Contains(sBullDt))
                        {
                            SellPosition(MyUtil.ConvertToDatetime(sDate), corpBase, footData);
                            corpBase.HasPosition = false;
                            break;
                        }
                    }
                }
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
                    iCalc += (Convert.ToInt32(trade[3]) - Convert.ToInt32(trade[2])) * Convert.ToInt32(trade[4]);
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
            Console.WriteLine(corpBase.CorpNo + "[SELL]:" + sDate + "@" + corpBase.BairPrice);
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
                sw.WriteLine(string.Join(",", new string[] { "BuyD", "SellD", "BuyV", "SellV", "Unit", "Total", }));
                foreach (var corp in ((VerifiedResultsBase)vrfRslt).CorpList)
                {
                    sw.WriteLine(string.Join(",", corp.CorpNo));
                    foreach (string[] valData in ((CorpBase)corp).TradeValList)
                    {
                        sw.WriteLine(string.Join(",", valData));
                        lgSum += (Convert.ToInt64(valData[3]) - Convert.ToInt64(valData[2]));
                    }
                    sw.WriteLine(string.Join(",", new string[] { "AccmTotal", "", "", "", "", lgSum.ToString(), }));
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
