using BluetoothService.BluetoothServices;
using System.Collections.Generic;

namespace ShellTemperature.Models
{
    public sealed class BluetoothDevice : Device
    {
        /// <summary>
        /// Stores all the temperature points regardless if they are outliers or not.
        /// </summary>
        public List<double> AllTemperatureReadings { get; set; }

        public IReceiverBluetoothService BluetoothService { get; set; }

        public FoundBluetoothDevice FoundBluetoothDevice { get; set; }

        public BluetoothDevice()
        {

        }

        public BluetoothDevice(FoundBluetoothDevice device, string deviceName, string deviceAddress) : base(deviceName, deviceAddress)
        {
            BluetoothService = new ReceiverBluetoothService();
            FoundBluetoothDevice = device;
            AllTemperatureReadings = new List<double>();
        }
    }
}