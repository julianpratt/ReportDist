using System;
using Mistware.Utils;


namespace ReportDist.Models
{
    public class CreateReportPostModel 
    {
        public string Company    { get; set; } 
        public string Year       { get; set; } 
        public string Division   { get; set; }
        public string Department { get; set; }
        public string Team       { get; set; }
        public bool   CheckNDT   { get; set; }
        public string NDTNo      { get; set; }
        public string ReportType { get; set; }

        public string ReportNo
        {
            get
            {
                string reportNo = null;

                string origin   = null;
                if      (Team       != null) origin = Team;
                else if (Department != null) origin = Department;
                else if (Division   != null) origin = Division;
                else throw new Exception("Unable to determine origin code");
                if (Year == null || Year.Length < 2) 
                    throw new Exception("Year is null or too short in CreateReportPostModel");
                if (Company == null) throw new Exception("Company is null in CreateReportPostModel");

                if (CheckNDT)
                {
                    if (NDTNo == null)      throw new Exception("NDTNo is null in CreateReportPostModel");
                    if (!NDTNo.IsInteger()) throw new Exception("NDTNo is not an integer in CreateReportPostModel");
                    if      (Company == "PT")  reportNo = String.Format("PT/{0}/LF{1}/",    Year, NDTNo);
                    else if (Company == "EEN") reportNo = String.Format("EEN/{0}/OSI/{1}/", Year, NDTNo);
                    else                       reportNo = String.Format("{0}/{1}/{2}/{3}/", Company, Year, origin, NDTNo);
                }
                else
                {
                    reportNo = String.Format("{0}/{1}/{2}/", Company, Year, origin);
                }

                return reportNo;
            }
        }

    }
}