using System;

namespace BluetoothService.Models
{
    public class DeviceReading
    {
        public double Temperature { get; set; }

        public DateTime RecordedDateTime { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }
    }
}