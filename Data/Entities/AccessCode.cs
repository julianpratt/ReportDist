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

        [Required, Column("AccessKey", TypeName = "char"), MaxLength(4)]
        public string Code         { get; set; }

        [Required, MaxLength(50)]
        public string Title         { get; set; }

        [Column(TypeName = "bit")]
        public bool   Disabled      { get; set; }
    }
}