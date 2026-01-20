using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleToPostgres.Models
{
    [Table("departments")]
    public class Departments
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(10)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public int DisplayOrder { get; set; }

        public DateTime? CreatedAt { get; set; }

    }
}
