using System;

namespace ShellTemperature.Models
{
    public sealed class WifiDevice : Device
    {
        public WifiDevice()
        { }

        public WifiDevice(string deviceName, string deviceAddress, DateTime start) : base(deviceName, deviceAddress)
        {
            StartRecordingTime = start;
        }

        public DateTime StartRecordingTime { get; set; }
    }
}