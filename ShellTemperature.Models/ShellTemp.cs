using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShellTemperature.Models
{
    /// <summary>
    /// Ladle shell recordings
    /// </summary>
    public class ShellTemp
    {
        /// <summary>
        /// The id of the temperature reading
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public Guid Id { get; set; }

        /// <summary>
        /// The temperature reading of the ladle shell
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// The date and time the ladle shell temperature was recorded.
        /// </summary>
        public DateTime RecordedDateTime { get; set; }

        /// <summary>
        /// The device the temperature was recorded on
        /// </summary>
        public DeviceInfo Device { get; set; }
    }
}
