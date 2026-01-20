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
        [Column("department_id")]
        public int DepartmentId { get; set; }

        [Required]
        [Column("new_patients_count")]
        public int NewPatientsCount { get; set; }

        [Required]
        [Column("returning_patients_count")]
        public int ReturningPatientsCount { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

    }
}
