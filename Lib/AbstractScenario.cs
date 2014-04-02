using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BackTest.Lib
{
    public abstract class AbstractScenario
    {
        string[][] sArrRole = new string[][] { };
        protected StaticData StaticData { get; set; }

        protected abstract AnalizedResults Analize(Rule rule, Hashtable dicCrpValList);
        protected abstract VerifiedResults VerifyResults(AnalizedResults analRslt);
        protected abstract Report CreateReports(VerifiedResults vrfRslt);
        protected abstract Rule SetupRules();
        protected abstract StaticData GetStaticData();
        protected abstract Configuration ReadConfigurations();
        protected abstract Hashtable ReadFiles(Configuration config);

        public void run(string[] args)
        {
            //this.CorpList = new List<Corp>();

            // Read Rules
            Rule rule = SetupRules();

            // Get Static Data
            this.StaticData = GetStaticData();

            // Read Configurations
            Configuration configs = ReadConfigurations();

            // Read Files
            Hashtable dicCorpValList = ReadFiles(configs);

            // Start Analizing
            AnalizedResults analRslt = Analize(rule, dicCorpValList);
            analRslt.Config = configs;

            // Verify Results
            VerifiedResults vrfRslt = VerifyResults(analRslt);
            vrfRslt.Config = configs;

            // Create Reports
            Report report = CreateReports(vrfRslt);
        }
    }
}
