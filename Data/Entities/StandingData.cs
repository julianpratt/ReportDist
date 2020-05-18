using System.Collections.Generic;

namespace ReportDist.Data
{
    public class StandingData
    {
        public IEnumerable<AccessCode>    AccessCodes    { get; set; }

        public IEnumerable<Company>       Companies      { get; set; }
        
        public IEnumerable<ReportType>    ReportTypes    { get; set; }

        public IEnumerable<SecurityLevel> SecurityLevels { get; set; }
 
        public IEnumerable<string>        Years          { get; set; }
        
        public IEnumerable<string>        ShortYears     { get; set; }

    }
}
