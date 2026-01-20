using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleToPostgres.Models
{
    [Table("sales")]
    public class Sales
    {
        [Key]
        [MaxLength(20)]
        public string DoctorCode { get; set; }

        [Key]
        [MaxLength(7)]
        public string YearMonth { get; set; }

        [Required]
        public long OutpatientSales { get; set; }

        [Required]
        public long InpatientSales { get; set; }

        public DateTime? UpdatedAt { get; set; }

    }
}
