using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mistware.Utils;

namespace ReportDist.Data
{
    [Table("ReportCirculation")]
    public class Circulation : IID
    {    
        public Circulation()
        {
            CirculationId = 0;
            Name          = "";
            Email         = "";
            Address       = "";
            ToCcBcc       = eToCcBcc.TO;
            Delivery      = "ES";
            PendingId     = 0;
            RecipientID   = 0;
            State         = 1;
        }   
        public enum eToCcBcc : int { TO = 1, CC, BCC };

        [NotMapped]
        public int       Id
        {
            get { return CirculationId;  }
            set { CirculationId = value; }
        } 

        [Key, Column("ReportCirculationId")]
        public int      CirculationId { get; set; }

        [Required, MaxLength(65, ErrorMessage="The Name field cannot have more than 65 characters.")]
        public string   Name          { get; set; } 

        [MaxLength(100, ErrorMessage="The Email field cannot have more than 100 characters.")]
        public string   Email         { get; set; }

        [MaxLength(300, ErrorMessage="The Address field cannot have more than 300 characters.")]
        public string   Address       { get; set; }

        [NotMapped]
        public string   AddressTrunc  
        { 
            get
            {
                if (Address == null) return null;
                if (Address.Length <= 50) return Address;
                return Address.Left(50) + "...";
            } 
        }

        [Column("ToCcBcc")]
        public int      ToCcBccDB     { get; set; }  

        [NotMapped]
        public eToCcBcc ToCcBcc       
        { 
            get { return (eToCcBcc)Convert.ToInt32(ToCcBccDB); }
            set { ToCcBccDB = Convert.ToInt32(value); } 
        }

        [NotMapped]
        public string   ToCcBccText 
        { 
            get 
            {
                if      (ToCcBcc == eToCcBcc.TO) return "To";
                else if (ToCcBcc == eToCcBcc.CC) return "CC";
                else                             return "BCC"; 
            } 
            set
            {
            	if      (value == "To" ) ToCcBcc = eToCcBcc.TO;
                else if (value == "CC" ) ToCcBcc = eToCcBcc.CC;
                else                     ToCcBcc = eToCcBcc.BCC; 
            }
        }

        public string[] ToCcBccList = new[] { "To", "CC", "BCC" };
       
        [Required, Column(TypeName = "char(8)"), StringLength(8)]
        public string   Delivery
        {
            get
            {
                if (ElecDeliveryCode  == null) return PaperDeliveryCode;
                if (PaperDeliveryCode == null) return ElecDeliveryCode;
                return ElecDeliveryCode + "," + PaperDeliveryCode;
            }
            set
            {
                ElecDeliveryCode  = null;
                PaperDeliveryCode = null;

                if      (value == null) { } // Neither set
                else if (value.Trim().Length == 5)  // Both set
                {
                    ElecDeliveryCode  = value.Substring(0,2);
                    PaperDeliveryCode = value.Substring(3,2);
                }
                else if (value[0] == 'E')
                {
                    ElecDeliveryCode = value.Trim();
                }
                else if (value[0] == 'P')
                {
                    PaperDeliveryCode = value.Trim();
                }
            }
        }

        [NotMapped]
        private string elecDeliveryCode = null;

        [NotMapped]
        public string ElecDeliveryCode
        {
            get { return elecDeliveryCode; }
            set { elecDeliveryCode = (value != null && value.Length == 2) ? value : null; }
        }

        [NotMapped]
        public string ElecDelivery
        {
            get
            {
                if      (ElecDeliveryCode == "ES") return "Summary";
                else if (ElecDeliveryCode == "EF") return "Full";
                else if (ElecDeliveryCode == "EC") return "CD";
                else                               return "None";
            }
            set
            {
                string s = value.ToUpper();
                if      (s == "SUMMARY") ElecDeliveryCode = "ES";
                else if (s == "FULL"   ) ElecDeliveryCode = "EF";
                else if (s == "CD"     ) ElecDeliveryCode = "EC";
                else                     ElecDeliveryCode = null;
            }
        }

        [NotMapped]
        private string paperDeliveryCode = null;

        [NotMapped]
        public string PaperDeliveryCode
        {
            get { return paperDeliveryCode; }
            set { paperDeliveryCode = (value != null && value.Length == 2) ? value : null; }
        }

        [NotMapped]
        public string PaperDelivery
        {
            get
            {
                if      (PaperDeliveryCode == "PS") return "Summary";
                else if (PaperDeliveryCode == "PF") return "Full";
                else                                return "None";
            }
            set
            {
                string s = value.ToUpper();
                if      (s == "SUMMARY") PaperDeliveryCode = "PS";
                else if (s == "FULL"   ) PaperDeliveryCode = "PF";
                else                     PaperDeliveryCode = null;
            }
        }

        [Required, Column("ReportsPendingID")]
        public int      PendingId     { get; set; }
        
        public int?     RecipientID   { get; set; }

        [Required]
        public int      State         { get; set; }
        
        public static string Tooltip(string code)
        {
            string sTip = "";

            if (code.Length == 2 || code.Length == 5)
            {
                string elecCode = (code.Length == 5) ? code.Substring(0, 2) : ((code.Substring(0, 1) == "E") ? code : null);
                string paperCode = (code.Length == 5) ? code.Substring(3, 2) : ((code.Substring(0, 1) == "P") ? code : null);

                if (elecCode == "ES") sTip += "Electronic (Summary)";
                else if (elecCode == "EF") sTip += "Electronic (Full)";
                else if (elecCode == "EC") sTip += "Electronic (Summary with CD)";

                if (sTip.Length > 0 && paperCode != null) sTip += " & ";

                if (paperCode == "PS") sTip += "Paper (Summary)";
                else if (paperCode == "PF") sTip += "Paper (Full)";
            }

            return sTip;
        }

        public string ValidateMessage()
        {
            if (ElecDeliveryCode != null && Email.Trim().Length == 0)
            {
                return "A valid email address is required for this recipient, \\nin order to distribute the report electronically.";
            }
            if (PaperDeliveryCode != null && Address.Trim().Length == 0)
            {
                return "A valid postal address is required for this recipient, \\nin order to distribute the report.";
            }
            if (ElecDeliveryCode == null && PaperDeliveryCode == null)
            {
                return "Please select a method of report distribution for this recipient.\\nPaper or Electronic, Summary or Full?";
            }
            return null;
        }  

    }
}