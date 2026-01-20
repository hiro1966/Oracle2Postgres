using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OracleToPostgres.Models
{
    [Table("messages")]
    public class Messages
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public string Content { get; set; }

        public DateTime? CreatedAt { get; set; }

    }
}
