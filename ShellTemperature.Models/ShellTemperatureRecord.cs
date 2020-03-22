using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using ShellTemperature.Data;

namespace ShellTemperature.Models
{
    public class ShellTemperatureRecord : ModelBase
    {
        [Display(Order = 0)]
        public Guid Id { get; set; }
        [Display(Order = 1)]
        public double Temperature { get; set; }
        [Display(Order = 2)]
        public DateTime RecordedDateTime { get; set; }
        [Display(Order = 3)]
        public float? Latitude { get; set; }
        [Display(Order = 4)]
        public float? Longitude { get; set; }

        /// <summary>
        /// The device the temperature was recorded on
        /// </summary>
        [Display(Order = 5)]
        public DeviceInfo Device { get; set; }

        private string _comment;
        [Display(Order = 6)]
        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }

        private string _position;
        [Display(Order = 7)]
        public string Position
        {
            get => _position;
            set
            {
                _position = value;
                OnPropertyChanged(nameof(Position));
            }
        }

        /// <summary>
        /// Property indicating if the record has come from the sd card or live data
        /// </summary>
        [Display(Order = 8)]
        public bool IsFromSdCard { get; set; }

        public ShellTemperatureRecord()
        {

        }

        public ShellTemperatureRecord(Guid id, double temperature, DateTime recordedDateTime, float?
            latitude, float? longitude, DeviceInfo deviceInfo)
        {
            Id = id;
            Temperature = temperature;
            RecordedDateTime = recordedDateTime;
            Latitude = latitude;
            Longitude = longitude;
            Device = deviceInfo;
        }

        public ShellTemperatureRecord(Guid id, double temperature, DateTime recordedDateTime, float?
            latitude, float? longitude, DeviceInfo deviceInfo, string comment)
        : this(id, temperature, recordedDateTime, latitude, longitude, deviceInfo)
        {
            Comment = comment;
        }

        public ShellTemperatureRecord(Guid id, double temperature, DateTime recordedDateTime, float?
            latitude, float? longitude, DeviceInfo deviceInfo, bool isFromSdCard)
            : this(id, temperature, recordedDateTime, latitude, longitude, deviceInfo)
        {
            IsFromSdCard = isFromSdCard;
        }

        public ShellTemperatureRecord(Guid id, double temperature, DateTime recordedDateTime, float?
            latitude, float? longitude, DeviceInfo deviceInfo, string comment, bool isFromSdCard)
            : this(id, temperature, recordedDateTime, latitude, longitude, deviceInfo, comment)
        {
            IsFromSdCard = isFromSdCard;
        }

        /// <summary>
        /// For a given shell temperature record property
        /// extract the name of the property in a correctly
        /// formatted order
        /// </summary>
        /// <param name="record">Shell temperature record</param>
        /// <returns>Return string array of column headers in formatted order</returns>
        public string[] GetHeaders()
        {
            // Get the properties for the type
            PropertyInfo[] properties = this.GetType().GetProperties().ToArray();
            string[] headers = new string[properties.Length];

            // Extract the order and name and insert into array at correct position
            foreach (var property in properties)
            {
                Attribute[] t = property.GetCustomAttributes(typeof(DisplayAttribute)).ToArray();
                if (t.Length != 1) continue;

                // Get the order for the property
                int order = ((DisplayAttribute)t[0]).Order;
                headers[order] = property.Name;
            }

            return headers;
        }
    }
}