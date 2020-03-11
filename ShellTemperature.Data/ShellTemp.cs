using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShellTemperature.Data
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
        [Required]
        public double Temperature { get; set; }


        /// <summary>
        /// The date and time the ladle shell temperature was recorded.
        /// </summary>
        [Required]
        public DateTime RecordedDateTime { get; set; }

        /// <summary>
        /// The latitude co-ordinates the reading was taken
        /// </summary>
        public float? Latitude { get; set; }

        /// <summary>
        /// The longitude co-ordinates the reading was taken
        /// </summary>
        public float? Longitude { get; set; }

        /// <summary>
        /// The device the temperature was recorded on
        /// </summary>
        public DeviceInfo Device { get; set; }

        #region Constructors

        public ShellTemp()
        {

        }

        public ShellTemp(double temperature, DateTime dateTime, float? latitude, float?
            longitude, DeviceInfo deviceInfo)
        {
            Temperature = temperature;
            RecordedDateTime = dateTime;
            Latitude = latitude;
            Longitude = longitude;
            Device = deviceInfo;
        }

        public ShellTemp(Guid id, double temperature, DateTime dateTime, float? latitude, float?
            longitude, DeviceInfo deviceInfo)
        : this(temperature, dateTime, latitude, longitude, deviceInfo)
        {
            Id = id;
        }
        #endregion
    }
}
