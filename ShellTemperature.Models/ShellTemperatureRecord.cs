using System;
using System.ComponentModel.DataAnnotations;
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
    }
}