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
        [Column("ward_id")]
        public int WardId { get; set; }

        [Required]
        [Column("department_id")]
        public int DepartmentId { get; set; }

        [Required]
        [Column("current_patient_count")]
        public int CurrentPatientCount { get; set; }

        [Required]
        [Column("new_admission_count")]
        public int NewAdmissionCount { get; set; }

        [Required]
        [Column("discharge_count")]
        public int DischargeCount { get; set; }

        [Required]
        [Column("transfer_out_count")]
        public int TransferOutCount { get; set; }

        [Required]
        [Column("transfer_in_count")]
        public int TransferInCount { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

    }
}
