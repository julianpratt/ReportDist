using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportDist.Data
{
    [Table("AccessLookup")]
    public class AccessCode : IID
    {
        public AccessCode()
        {
            Id        = 0;
            Code      = "";
            Title     = "";
        }

        [Key, Column("AccessLookupID")]
        public int    Id           { get; set; }

        [NotMapped]
        private string accessKey   { get; set; }

        [Required, Column("AccessKey", TypeName = "char(4)"), MaxLength(4)]
        public string Code         
        { 
            get { return accessKey; } 
            set { accessKey = value.TrimEnd(); } 
        }

        [Required, MaxLength(50)]
        public string Title         { get; set; }

        [Column(TypeName = "bit")]
        public bool   Disabled      { get; set; }
    }
}