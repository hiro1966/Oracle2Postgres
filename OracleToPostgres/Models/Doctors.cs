using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleToPostgres.Models
{
    [Table("doctors")]
    public class Doctors
    {
        [Key]
        [MaxLength(10)]
        [Column("code")]
        public string Code { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        [Column("department_code")]
        public string DepartmentCode { get; set; } = string.Empty;

        [Required]
        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

    }
}
