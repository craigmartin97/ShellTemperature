using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace ShellTemperature.Data
{
    public class ReadingComment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public Guid Id { get; set; }

        [Required]
        public string Comment { get; set; }

        public ReadingComment()
        {
            
        }

        public ReadingComment(string comment)
        {
            Comment = comment;
        }
    }
}