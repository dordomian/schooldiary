using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolDiary.Models
{
    public class Log
    {
        public int ID { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 3)]
        public string EntityName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string EntityStatus { get; set; }

        [Required]
        public int RowId { get; set; }

        [Required]
        [MaxLength]
        public string Changes { get; set; }

        [Required]
        [Timestamp]
        public byte[] Timestamp { get; set; }
    }
}