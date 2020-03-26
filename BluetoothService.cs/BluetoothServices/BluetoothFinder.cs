using BluetoothService.Models;
using InTheHand.Net.Sockets;
using System.Collections.Generic;
using System.Linq;

namespace BluetoothService.BluetoothServices
{
    public class BluetoothFinder : IBluetoothFinder
    {
        private readonly BluetoothConfiguration[] _temperatureDevices;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="temperatureDevices"></param>
        public BluetoothFinder(BluetoothConfiguration[] temperatureDevices)
        {
            _temperatureDevices = temperatureDevices;
        }

        /// <summary>
        /// Get all of the bluetooth devices 
        /// by a given name
        /// </summary>
        /// <returns></returns>
        public List<FoundBluetoothDevice> GetBluetoothDevices()
        {
            BluetoothDeviceInfo[] foundDevices = new BluetoothClient().DiscoverDevicesInRange(); //1000, true, true, false, true
            List<FoundBluetoothDevice> devices = new List<FoundBluetoothDevice>();

            foreach (BluetoothConfiguration s in _temperatureDevices)
            {
                List<BluetoothDeviceInfo> matches = foundDevices.Where(x => x.DeviceName.ToLower().Equals(s.Name.ToLower())).ToList();

                foreach (BluetoothDeviceInfo bluetoothDevice in matches)
                {
                    devices.Add(new FoundBluetoothDevice
                    {
                        Device = bluetoothDevice,
                        Client = new BluetoothClient(),
                        Configuration = s
                    });
                }
            }

            return devices;
        }
    }
}
