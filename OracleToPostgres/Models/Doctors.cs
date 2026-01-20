using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleToPostgres.Models
{
    [Table("doctors")]
    public class Doctors
    {
        [Key]
        [MaxLength(20)]
        public string Code { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(10)]
        public string DepartmentCode { get; set; }

        [Required]
        public int DisplayOrder { get; set; }

        public DateTime? CreatedAt { get; set; }

    }
}
