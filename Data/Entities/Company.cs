using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ReportDist.Data
{
    [Table("Company")]
    public class Company : IID
    {
        public Company()
        {
            Id        = 0;
            Code      = "";
            Name      = "";
            StartYear = 0;
        }

        [Key]
        public int    Id           { get; set; }

        [Required, MaxLength(10)]
        public string Code         { get; set; }

        [Required, MaxLength(100)]
        public string Name         { get; set; }

        [Required]
        public int    StartYear    { get; set; }
    }
}
