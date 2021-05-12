using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportDist.Data
{
    [Table("ReportType")]
    public class ReportType : IID
    {
        public ReportType()
        {
            Id        = 0;
            Code      = "";
            Name      = "";
        }

        [Key]
        public int    Id           { get; set; }

        [Required, MaxLength(10)]
        public string Code         { get; set; }

        [Required, MaxLength(100)]
        public string Name         { get; set; }
    }
}
