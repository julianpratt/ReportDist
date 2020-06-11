using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportDist.Data
{
    [Table("ReportsPending")]
    public class PendingReport : IID
    {
        public PendingReport()
        {
            Abstract      = "";
            Axess         = "";
            Author        = "";
            CID           = -1;
            CreationDate  = DateTime.Now;
            DateSent      = null;
            Deleted       = false;
            eFileName     = null;
            eFilePath     = null;
            JobNo         = "";
            ReportNo      = "";
            RecipientID   = -1;
            ReportType    = "";
            ReportYear    = "";
            Software      = "";
            SecurityLevel = -1;
            State         = 0;
            Title         = "";
        }

        [MaxLength(3000, ErrorMessage="The Abstract field cannot have more than 3,000 characters.")]
        public string    Abstract      { get; set; }

        [MaxLength(200, ErrorMessage="The Access Code field cannot have more than 200 characters.")]
        public string    Axess         { get; set; }

        [MaxLength(500, ErrorMessage="The Author field cannot have more than 500 characters.")]
        public string    Author        { get; set; }

        [NotMapped]
        private int      cid           { get; set; }
        public int?      CID
        {
            get { return cid; }
            set
            {
                if (value.HasValue) cid = value.Value;
                else cid = -1;
            }
        }

        [Required]
        public DateTime  CreationDate  { get; set; }
        public DateTime? DateSent      { get; set; }

        [Required, Column(TypeName = "bit")]
        public bool      Deleted       { get; set; }

        [MaxLength(255)]
        public string    eFileName     { get; set; }

        [MaxLength(255)]
        public string    eFilePath     { get; set; }

        [NotMapped]
        public string    FullName
        {
            get
            {
                if (eFileName == null || eFilePath == null) return "";
                return eFileName;
            }
        }

        [MaxLength(55, ErrorMessage="The Job Number(s) field cannot have more than 55 characters.")]
        public string    JobNo         { get; set; }
        
        [NotMapped]
        public int       Id
        {
            get { return PendingId;  }
            set { PendingId = value; }
        } 

        [Key,Column("ReportsPendingID")]
        public int       PendingId     { get; set; }

        [NotMapped]
        public bool      ReadOnly
        {
            get { return (State > 0); }
        }
        
        [NotMapped]
        private int      recipientId   { get; set; }
        public int?      RecipientID
        {
            get { return recipientId; }
            set
            {
                if (value.HasValue) recipientId = value.Value;
                else recipientId = -1;
            }
        }

        [Required, MaxLength(155)]
        public string    ReportNo      { get; set; }

        [NotMapped]
        public string    FullReportNo
        {
            get
            {
                return ReportNo + ReportType;
            }
                 
        }

        [NotMapped]
        private string   reportType    { get; set; }

        [Required, Column(TypeName = "char(10)"), StringLength(10)]
        public string    ReportType    
        { 
            get { return reportType; } 
            set { reportType = Convert.ToString(value).TrimEnd(); } 
        }

        [NotMapped]
        private string   reportYear    { get; set; }

        [Required, Column(TypeName = "char(10)"), StringLength(10)]
        public string    ReportYear    
        { 
            get { return reportYear; } 
            set { reportYear = Convert.ToString(value).TrimEnd(); } 
        }

        [MaxLength(155, ErrorMessage="The Software field cannot have more than 155 characters.")]
        public string    Software      { get; set; }

        [NotMapped]
        private int      securityLevel { get; set; }

        public int?      SecurityLevel
        {
            get { return securityLevel; }
            set
            {
                if (value.HasValue) securityLevel = value.Value;
                else                securityLevel = -1;
            }
        }
       
        [Required]
        public int       State         { get; set; }

        [MaxLength(1500, ErrorMessage="The Title field cannot have more than 1500 characters.")]
        public string    Title         { get; set; }

    }
}