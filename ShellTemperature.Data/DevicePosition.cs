using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShellTemperature.Data
{
    /// <summary>
    /// The device position relates to the position the device
    /// was reading in
    /// </summary>
    public class DevicePosition
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public Guid Id { get; set; }

        [Required]
        public string Position { get; set; }

        #region Constructors

        public DevicePosition()
        {

        }

        public DevicePosition(string position)
        {
            Position = position;
        }
        #endregion
    }
}