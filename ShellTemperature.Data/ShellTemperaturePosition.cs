using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShellTemperature.Data
{
    /// <summary>
    /// Links a shell temperature to a position
    /// </summary>
    public class ShellTemperaturePosition
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public Guid Id { get; set; }

        [Required]
        public ShellTemp ShellTemp { get; set; }

        [Required]
        public DevicePosition Position { get; set; }
    }
}