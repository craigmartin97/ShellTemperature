using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShellTemperature.Data
{
    public class SdCardShellTemperatureComment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public Guid Id { get; set; }

        [Required]
        public ReadingComment Comment { get; set; }

        [Required]
        public SdCardShellTemp SdCardShellTemp { get; set; }

        public SdCardShellTemperatureComment()
        {

        }

        public SdCardShellTemperatureComment(ReadingComment comment, SdCardShellTemp shellTemp)
        {
            Comment = comment;
            SdCardShellTemp = shellTemp;
        }
    }
}