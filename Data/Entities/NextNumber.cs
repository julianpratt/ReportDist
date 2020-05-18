using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportDist.Data
{
    [Table("NextNumber")]
    public class NextNumbers
    {
        public NextNumbers()
        {
            ReportYear = 0;
            NextNumber = 0;
        }

        [Key]
        public int    ReportYear   { get; set; }

        [Required]
        public int    NextNumber   { get; set; }
    }
}