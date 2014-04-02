using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTest.Lib.Base
{
    public class VerifiedResultsBase : VerifiedResults
    {
        public Dictionary<string, object> VerifiedResultArray { get; set; }
        public List<Corp> CorpList { get; set; }
    }
}
