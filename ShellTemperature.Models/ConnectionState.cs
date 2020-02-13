using BluetoothService.Enums;

namespace ShellTemperature.Models
{
    public class ConnectionState
    {
        public DeviceConnectionStatus IsConnected { get; set; }

        public string Message { get; set; }
    }
}