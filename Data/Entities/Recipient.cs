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
                        FirstName = value.Left(i).Left(50);
                        LastName  = value.Substring(i+1).Left(50);
                    }
                    else
                    {
                        FirstName = "";
                        LastName = value.Left(50);
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
                AddressLine1 = null;
                AddressLine2 = null;
                AddressLine3 = null;
                AddressLine4 = null;
                AddressLine5 = null;
                PostCode     = null;
                if (value.HasValue())
                {
                    string[] addresslines = value.Split('\n');
                    if (addresslines.Length == 1) {
                        /* Alternative parsing assuming no carriage returns */
                        addresslines = value.Split(',');
                    }
                    int n = addresslines.Length;
                    for (int i=0; i<n; ++i) 
                    {
                        addresslines[i]=TrimComma(addresslines[i]);
                        if (addresslines[i].Length > 50) 
                          Log.Me.Warn("Address truncation for Recipient " + Name + " - this value will be truncated to 50 characters: " + addresslines[i]);
                    }                      
                    if (addresslines[n-1].Length <= 10) 
                    {
                        // Final line is 10 char or less, so it is assumed to be the postcode
                        PostCode = addresslines[n-1];
                        --n; // We've used the final line, so don't give it to an address line
                    }
                    if (n > 0) AddressLine1 = addresslines[0].Left(50);
                    if (n > 1) AddressLine2 = addresslines[1].Left(50);
                    if (n > 2) AddressLine3 = addresslines[2].Left(50);
                    if (n > 3) AddressLine4 = addresslines[3].Left(50);
                    if (n > 4) AddressLine5 = addresslines[4].Left(50);
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
                address += ( (address.Length > 0) ? ",\r\n" : "" ) + TrimComma(line);
            }
            return address;
        }
        private static string TrimComma(string sin)
        {
            string s = sin.TrimEnd();
            if (s.Right(1) == ",") return s.Left(s.Length - 1);
            else                   return s;
        }

    }
}