using System;
using System.Collections.Generic;
using System.Text;
using Mistware.Utils;

namespace ReportDist.Data
{
    public class TransferReport
    {
        private TransferReport() { } // Parameterless constructor required for serialization.
        public TransferReport(PendingReport pr)
        {
            this.Abstract      = pr.Abstract;
            this.Authors       = ListSplit(pr.Author, ';');
            this.Axess         = ListSplit(pr.Axess, '/');
            this.CatNo         = pr.PendingId + 300000;
            this.DateIssued    = DateTime.Now.Midnight();
            this.eFileName     = pr.eFileName;
            this.JobNumber     = pr.JobNo;
            this.MaterialType  = 5;
            this.ReportNumber  = pr.FullReportNo.Trim();
            this.ReportYear    = pr.ReportYear.ToInteger();
            this.SecurityLevel = pr.SecurityLevel.HasValue ? pr.SecurityLevel.Value : -1;
            this.Software      = pr.Software;
            this.Title         = pr.Title;             
        }
        public string       Abstract      { get; set; }
        public List<string> Authors       { get; set; }
        public List<string> Axess         { get; set; }
        public int          CatNo         { get; set; }
        public DateTime     DateIssued    { get; set; }
        public string       eFileName     { get; set; }
        public string       JobNumber     { get; set; }
        public int          MaterialType  { get; set; }
        public string       ReportNumber  { get; set; }
        public int          ReportYear    { get; set; }
        public int          SecurityLevel { get; set; }
        public string       Software      { get; set; }       
        public string       Title         { get; set; }

        private void AddElement(StringBuilder sb, string name, string value)
        {
            sb.Append(String.Format("  <{0}>{1}</{0}>\n", name, value));
        }

        public string ToXML()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<root>\n");
            sb.Append("  <Report>\n");
            AddElement(sb, "Id",            this.CatNo.ToString());
            AddElement(sb, "ReportNumber",  this.ReportNumber);
            AddElement(sb, "JobNumber",     EscapeXML(this.JobNumber));
            AddElement(sb, "DateIssued",    this.DateIssued.ToString("yyyy-MM-dd"));
            AddElement(sb, "ReportYear",    this.ReportYear.ToString());
            AddElement(sb, "Title",         EscapeXML(this.Title));
            AddElement(sb, "Abstract",      EscapeXML(this.Abstract));
            foreach (string author in this.Authors) AddElement(sb, "Author", EscapeXML(author));
            AddElement(sb, "SecurityLevel", this.SecurityLevel.ToString());
            AddElement(sb, "Software",      EscapeXML(this.Software));
            AddElement(sb, "Publisher",     EmailConfig.Me.Company);
            foreach (string axess in this.Axess) AddElement(sb, "Axess", (axess == "UKEM") ? "GEN": axess);
            AddElement(sb, "eFileName",     EscapeXML(this.eFileName));
            AddElement(sb, "LastUpdated",   this.DateIssued.ToString("yyyy-MM-dd"));
            sb.Append("  </Report>\n");
            sb.Append("</root>\n");
            return sb.ToString();
        }

        private List<string> ListSplit(string list, char delimiter)
        {
            if (list == null) return null;
            string[] l = list.Split(delimiter);
            List<string> ret = new List<string>();
            foreach (string s in l) ret.Add(s);
            return ret;  
        }

        /*
        private string EncodeChars(string s)
        {
            char[] chars = { '&', '<', '>', '"', '\'', '\n' };
            string sout = s;
            int i = 0;
            int j = 0;
            string rep = "";

            while ((j = sout.IndexOfAny(chars, i)) > 0)
            {
                char c = sout[j];
                if (c == '&') rep = "&amp;";
                else if (c == '<')  rep = "&lt;";
                else if (c == '>')  rep = "&gt;";
                else if (c == '"')  rep = "&quot;";
                else if (c == '\'') rep = "&apos;";
                else if (c == '\n') rep = "&#13;";
                sout = sout.Left(j) + rep + sout.Substring(j + 1);
                i = j + 1;
            }
            return sout;
        }
        */

        private string EscapeXML(string s)
        {
            if (s==null) return "";

            StringBuilder sb = new StringBuilder();
            string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 !#$%()*+,-./:;=?[]^_{}|~";

            foreach (char c in s)
            {
                if (valid.IndexOf(c) > -1) sb.Append(c);
                else if (c == '<')         sb.Append("&lt;");
                else if (c == '>')         sb.Append("&gt;");
                else if (c == '&')         sb.Append("&amp;");
                else if (c == '\'')        sb.Append("&apos;");
                else if (c == '\\')        sb.Append("&#x5C;");
                else if (c == '"')         sb.Append("&quot;");
                else 
                {
                    string temp = "&#x" + String.Format("{0:X}", Convert.ToUInt32(c)) + ";";
                    if (temp != "&#x1F;" && temp != "&#xB;" && temp != "&#x1;" && temp != "&#xDBC0;" && temp != "&#xDC79;" 
                     && temp != "&#xC;"  && temp != "&#x2;" && temp != "&#xA;") 
                        sb.Append(temp);
                }                      
            }

            return sb.ToString();
        }

    }


}