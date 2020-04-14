using System.Collections.Generic;

namespace BluetoothService.BluetoothServices
{
    /// <summary>
    /// INterface to find and connect to local bluetooth devices
    /// </summary>
    public interface IBluetoothFinder
    {
        List<FoundBluetoothDevice> GetBluetoothDevices();
    }
}
