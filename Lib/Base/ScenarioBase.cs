using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BackTest.Lib.Base
{
    public class ScenarioBase : AbstractScenario
    {
        protected override Rule SetupRules()
        {
            Rule rule = new RuleBase();
            return rule;
        }

        protected override Configuration ReadConfigurations()
        {
            string sDirRoot = System.Configuration.ConfigurationManager.AppSettings["Dir"];
            return new ConfigurationBase() { DocRoot = sDirRoot };
        }

        protected override Hashtable ReadFiles(Configuration config)
        {
            var retDic = new Hashtable();
            var dirInfo = new DirectoryInfo(config.DocRoot);
            foreach (var item in dirInfo.GetFiles())
            {
                using (var sr = new StreamReader(item.FullName, Encoding.Default))
                {
                    retDic.Add(item.Name, MyUtil.CsvToArrayList(sr.ReadToEnd()));
                }
            }
            return retDic;
        }

        protected override StaticData GetStaticData()
        {
            return new StaticDataBase();
        }

        protected virtual Corp SetupCorp(object baseData)
        {
            var corpData = (DictionaryEntry)baseData;
            return new CorpBase
            {
                CorpValList = (ArrayList)corpData.Value,
                CorpNo = (string)((ArrayList)corpData.Value).Cast<ArrayList>().FirstOrDefault()[0],
            };
        }

        protected virtual Corp SetupCorp(object baseData, StaticData stData)
        {
            CorpBase corpBase = (CorpBase)this.SetupCorp(baseData);
            //corpBase.AmountUnit = Convert.ToInt32(((StaticDataBase)stData).CorpStaticData[corpBase.CorpNo]);
            return corpBase;
        }

        protected override AnalizedResults Analize(Rule rule, Hashtable dicCrpValList)
        {
            CorpBase corpBase = null;
            List<Corp> corpList = new List<Corp>();
            // CORP TRADE HISTORY LIST
            var dicTrdHisList = new Hashtable();
            foreach (DictionaryEntry corpData in dicCrpValList)
            {
                corpBase = (CorpBase)this.SetupCorp(corpData, this.StaticData);
                foreach (ArrayList footData in corpBase.CorpValList)
                {
                    AnalizeFootData(rule, footData, corpBase);
                }
                PostAnalizeFootData(corpBase, dicTrdHisList);
                corpList.Add(corpBase);
                Console.WriteLine("-----------------------------------------------------------");
            }
            return new AnalizedResultsBase() { CorpList = corpList, CorpTradeHistorys = dicTrdHisList };
        }

        protected virtual void AnalizeFootData(Rule rule, ArrayList footData, CorpBase corp) { }
        protected virtual void PostAnalizeFootData(CorpBase corp, Hashtable dicTrdHisList)
        {
            dicTrdHisList.Add(corp.CorpNo, corp.TradeValList);
        }

        protected override VerifiedResults VerifyResults(AnalizedResults analRslt)
        {
            var retRslt = new VerifiedResultsBase();
            retRslt.CorpList = ((AnalizedResultsBase)analRslt).CorpList;
            return retRslt;
        }

        protected Hashtable prepareCorpsValues()
        {
            // Read Configurations
            Configuration configs = ReadConfigurations();

            // Read Files
            return ReadFiles(configs);
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
            Console.ReadLine();
            return null;
        }
    }
}
