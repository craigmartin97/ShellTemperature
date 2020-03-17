﻿using InTheHand.Net.Sockets;

namespace BluetoothService.cs.BluetoothServices
{
    /// <summary>
    /// Bluetooth device allows connection to the bluetooth device
    /// with the devices information, the access client and the configuration information
    /// for the device
    /// </summary>
    public class BluetoothDevice
    {
        public BluetoothDeviceInfo Device { get; set; }

        public BluetoothClient Client { get; set; }

        public BluetoothConfiguration Configuration { get; set; }
    }
}
