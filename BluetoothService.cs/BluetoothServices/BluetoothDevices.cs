using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Text;

namespace BluetoothService.cs.BluetoothServices
{
    public class BluetoothDevice
    {
        public BluetoothDeviceInfo Device { get; set; }

        public BluetoothClient Client { get; set; }
    }
}
