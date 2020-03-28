using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using ShellTemperature.Data;

namespace ShellTemperature.Models
{
    public class ShellTemperatureRecordAPI : ModelBase
    {
        public Guid Id { get; set; }

        public double Temperature { get; set; }

        public DateTime? RecordedDateTime { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }

        public DeviceInfo DeviceInfo { get; set; }
    }
}