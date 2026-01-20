using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleToPostgres.Models
{
    [Table("permissions")]
    public class Permissions
    {
        [Key]
        [MaxLength(10)]
        [Column("job_type_code")]
        public string JobTypeCode { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("job_type_name")]
        public string JobTypeName { get; set; } = string.Empty;

        [Required]
        [Column("level")]
        public int Level { get; set; }

    }
}
