using System;

namespace BluetoothService.Models
{
    public class DeviceReading
    {
        public double Temperature { get; set; }

        public DateTime RecordedDateTime { get; set; }

        public float? Latitude { get; set; }

        public float? Longitude { get; set; }

        ///////////////////////////////////////

        public double SdTemperature { get; set; }
        public DateTime SdRecordedDateTime { get; set; }
        public float? SdLatitude { get; set; }
        public float? SdLongitude { get; set; }

        public bool HasSdCardData { get; set; }
    }
}