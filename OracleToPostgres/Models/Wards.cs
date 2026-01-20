using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleToPostgres.Models
{
    [Table("wards")]
    public class Wards
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(10)]
        public string Code { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required]
        public int DisplayOrder { get; set; }

        public DateTime? CreatedAt { get; set; }

    }
}
