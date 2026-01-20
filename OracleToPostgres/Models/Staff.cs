using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleToPostgres.Models
{
    [Table("staff")]
    public class Staff
    {
        [Key]
        [MaxLength(20)]
        public string Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(2)]
        public string JobTypeCode { get; set; }

        public DateTime? CreatedAt { get; set; }

    }
}
