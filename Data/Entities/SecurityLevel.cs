using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;
using Mistware.Utils;

namespace ReportDist.Data
{
    [Table("SecurityLevel")]
    public class SecurityLevel : IID
    {
        public SecurityLevel()
        {
            Id   = 0;
            Code = 0;
            Text = "";
        }

        [NotMapped]
        public int    Id
        {
            get { return SecurityLevelID;  }
            set { SecurityLevelID = value; }
        } 

        [Key]
        public int    SecurityLevelID  { get; set; }

        [Required]
        public int    Code             { get; set; }

        [Required, MaxLength(50)]
        public string Text             { get; set; }

    }
}
