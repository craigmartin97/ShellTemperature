using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShellTemperature.Data
{
    /// <summary>
    /// Shell temperature comment allows users to add comments
    /// to a shell temperature
    /// </summary>
    public class ShellTemperatureComment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public Guid Id { get; set; }

        [Required]
        public ReadingComment Comment { get; set; }

        [Required]
        public ShellTemp ShellTemp { get; set; }

        public ShellTemperatureComment()
        {

        }

        public ShellTemperatureComment(ReadingComment comment, ShellTemp shellTemp)
        {
            Comment = comment;
            ShellTemp = shellTemp;
        }
    }
}