using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleToPostgres.Models
{
    [Table("sales")]
    public class Sales
    {
        [Key]
        [MaxLength(10)]
        [Column("doctor_code")]
        public string DoctorCode { get; set; } = string.Empty;

        [Key]
        [MaxLength(7)]
        [Column("year_month")]
        public string YearMonth { get; set; } = string.Empty;

        [Required]
        [Column("outpatient_sales")]
        public decimal OutpatientSales { get; set; }

        [Required]
        [Column("inpatient_sales")]
        public decimal InpatientSales { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

    }
}
