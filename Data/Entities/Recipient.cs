using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;
using Mistware.Utils;

namespace ReportDist.Data
{
    [Table("Recipient")]
    public class Recipient : IID
    {
        public Recipient()
        {
            UserName = "";
            FirstName = "";
            LastName = "";
            Email = "";
            JobTitle = "";
            Address = "";
            Deleted = false;
        }

        [NotMapped]
        public int    Id
        {
            get { return RecipientID;  }
            set { RecipientID = value; }
        } 

        [Key]
        public int    RecipientID  { get; set; }

        [MaxLength(100)]
        public string UserName     { get; set; }

        [MaxLength(50)]
        public string FirstName    { get; set; }

        [MaxLength(50)]
        public string LastName     { get; set; }

        [MaxLength(100)]
        public string Email        { get; set; }

        [MaxLength(50)]
        public string JobTitle     { get; set; }

        [MaxLength(50)]
        public string AddressLine1 { get; set; }

        [MaxLength(50)]
        public string AddressLine2 { get; set; }

        [MaxLength(50)]
        public string AddressLine3 { get; set; }

        [MaxLength(50)]
        public string AddressLine4 { get; set; }

        [MaxLength(50)]
        public string AddressLine5 { get; set; }

        [MaxLength(10)]
        public string PostCode     { get; set; }

        [Column(TypeName = "bit")]
        public bool   Deleted      { get; set; }

        [NotMapped]
        public string Name
        {
            get
            {
                if (FirstName == null && LastName == null) return null;
                else if (FirstName == null) return LastName;
                else if (LastName  == null) return FirstName;
                else return (FirstName.Trim() ?? "") + " " + (LastName.Trim() ?? "");
            }
            set
            {
                if (value == null)
                {
                    FirstName = null;
                    LastName  = null;
                }
                else
                {
                    int i = value.IndexOf(' ');
                    if (i > 0)
                    {
                        FirstName = value.Left(i);
                        LastName  = value.Substring(i+1);
                    }
                    else
                    {
                        FirstName = "";
                        LastName = value;
                    }       
                }
                             
            }
        }

        [NotMapped]
        public string Address
        {
            get
            {
                string address = "";
                address = AddressAppendLine(address, AddressLine1);
                address = AddressAppendLine(address, AddressLine2);
                address = AddressAppendLine(address, AddressLine3);
                address = AddressAppendLine(address, AddressLine4);
                address = AddressAppendLine(address, AddressLine5);
                address = AddressAppendLine(address, PostCode);
                return address;
            }
            set
            {
                if (value == null)
                {
                    AddressLine1 = null;
                    AddressLine2 = null;
                    AddressLine3 = null;
                    AddressLine4 = null;
                    AddressLine5 = null;
                    PostCode     = null;
                }
                else
                {
                    string[] addresslines = value.Split('\n');
                    AddressLine1 = AddressParseLine(addresslines, 1);
                    AddressLine2 = AddressParseLine(addresslines, 2);
                    AddressLine3 = AddressParseLine(addresslines, 3);
                    AddressLine4 = AddressParseLine(addresslines, 4);
                    AddressLine5 = AddressParseLine(addresslines, 5);
                    PostCode     = AddressParseLine(addresslines, 99).Left(10);
                }
            }

        }

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

        public MailAddress ToMailAddress()
        {
            return new MailAddress(this.Email, this.Name);
        }

        private static string AddressAppendLine(string address, string line)
        {
            if ( line == null || (line.Trim().Length == 0) )
            {
                //address += ( (address.Length > 0) ? ",\r\n" : "" );
            }
            else
            {
                address += ( (address.Length > 0) ? ",\r\n" : "" ) + TrimComma(line.Trim());
            }
            return address;
        }
        private static string AddressParseLine(string[] addresslines, int lineno)
        {
            string s = "";

            // N.B. Last line is always assumed to be the PostCode
            if (addresslines.Length > lineno) s = addresslines[lineno-1];
            else if (lineno == 99)            s = addresslines[addresslines.Length-1];

            return TrimComma(s.Trim());
        }

        private static string TrimComma(string s)
        {
            if (s.Right(1) == ",") return s.Left(s.Length - 1);
            else                   return s;
        }

    }
}