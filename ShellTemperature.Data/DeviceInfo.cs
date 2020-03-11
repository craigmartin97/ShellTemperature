using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShellTemperature.Data
{
    public class DeviceInfo
    {
        /// <summary>
        /// The id of the temperature reading
        /// </summary>
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public Guid Id { get; set; }

        [Required]
        public string DeviceAddress { get; set; }

        [Required]
        public string DeviceName { get; set; }

    }
}
