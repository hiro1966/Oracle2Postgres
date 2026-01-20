using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleToPostgres.Models
{
    [Table("inpatient_records")]
    public class InpatientRecords
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int WardId { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public int CurrentPatientCount { get; set; }

        [Required]
        public int NewAdmissionCount { get; set; }

        [Required]
        public int DischargeCount { get; set; }

        [Required]
        public int TransferOutCount { get; set; }

        [Required]
        public int TransferInCount { get; set; }

        public DateTime? CreatedAt { get; set; }

    }
}
