using System.Collections.Generic;
using BluetoothService.BluetoothServices;

namespace ShellTemperature.Models
{
    public class FindDevices
    {
        public IList<FoundBluetoothDevice> BluetoothDevices { get; set; }

        public IList<WifiDevice> WifiDevices { get; set; }
    }
}