using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using ReportDist.Data;

namespace ReportDist.Models
{
    public class PendingReportViewModel : PendingReport
    {
        public PendingReportViewModel(PendingReport pr, StandingData sd)
        {
            if (pr != null && sd != null)
            {
                this.Abstract      = pr.Abstract;
                this.Author        = pr.Author;
                this.Axess         = pr.Axess;
                this.CID           = pr.CID;
                this.CreationDate  = pr.CreationDate;
                this.DateSent      = pr.DateSent;
                this.Deleted       = pr.Deleted;
                this.eFileName     = pr.eFileName;
                this.eFilePath     = pr.eFilePath;
                this.JobNo         = pr.JobNo;
                this.RecipientID   = pr.RecipientID;
                this.ReportNo      = pr.ReportNo;
                this.ReportType    = pr.ReportType;
                this.ReportYear    = pr.ReportYear;
                this.SecurityLevel = pr.SecurityLevel;
                this.Software      = pr.Software;
                this.State         = pr.State;
                this.Title         = pr.Title;

                this.ReportTypes   = new List<SelectListItem>();
                foreach (ReportType type in sd.ReportTypes)
                {
                    this.ReportTypes.Add(new SelectListItem(type.Code, type.Code));
                }

                this.SecurityLevels = new List<SelectListItem>();
                foreach (SecurityLevel level in sd.SecurityLevels)
                {
                    this.SecurityLevels.Add(new SelectListItem(level.Text, level.Code.ToString()));
                }
            }
            
        }
        public List<SelectListItem> ReportTypes    { get; set; } 

        public List<SelectListItem> SecurityLevels { get; set; } 
 
    }
}