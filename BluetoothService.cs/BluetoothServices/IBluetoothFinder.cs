using BluetoothService.cs.BluetoothServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothService.BluetoothServices
{
    /// <summary>
    /// INterface to find and connect to local bluetooth devices
    /// </summary>
    public interface IBluetoothFinder
    {
        List<BluetoothDevice> GetBluetoothDevices();
    }
}
