using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleToPostgres.Models
{
    [Table("permissions")]
    public class Permissions
    {
        [Key]
        [MaxLength(2)]
        public string JobTypeCode { get; set; }

        [MaxLength(100)]
        public string JobTypeName { get; set; }

        [Required]
        public int Level { get; set; }

    }
}
