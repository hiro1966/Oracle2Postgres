using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleToPostgres.Models
{
    [Table("outpatient_records")]
    public class OutpatientRecords
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int NewPatientsCount { get; set; }

        [Required]
        public int ReturningPatientsCount { get; set; }

        public DateTime? CreatedAt { get; set; }

    }
}
