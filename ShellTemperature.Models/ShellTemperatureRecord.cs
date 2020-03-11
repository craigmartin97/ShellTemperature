using System;
using ShellTemperature.Data;

namespace ShellTemperature.Models
{
    public class ShellTemperatureRecord : ModelBase
    {
        public Guid Id { get; set; }
        public double Temperature { get; set; }

        public DateTime RecordedDateTime { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }

        /// <summary>
        /// The device the temperature was recorded on
        /// </summary>
        public DeviceInfo Device { get; set; }

        private string _comment;

        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }

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
    }
}