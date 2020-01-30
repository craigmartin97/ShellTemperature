using BluetoothService.cs.BluetoothServices;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BluetoothService.BluetoothServices
{
    public class BluetoothFinder : IBluetoothFinder
    {
        private readonly string[] temperatureDevices;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="temperatureDevices"></param>
        public BluetoothFinder(string[] temperatureDevices)
        {
            this.temperatureDevices = temperatureDevices;
        }

        /// <summary>
        /// Get all of the bluetooth devices 
        /// by a given name
        /// </summary>
        /// <returns></returns>
        public List<BluetoothDevice> GetBluetoothDevices()
        {
            BluetoothDeviceInfo[] foundDevices = new BluetoothClient().DiscoverDevices(1000, true, true, false, true);
            List<BluetoothDevice> devices = new List<BluetoothDevice>();

            foreach (string s in temperatureDevices)
            {
                List<BluetoothDeviceInfo> matches = foundDevices.Where(x => x.DeviceName.ToLower().Equals(s.ToLower())).ToList();

                foreach (BluetoothDeviceInfo bluetoothDevice in matches)
                {
                    devices.Add(new BluetoothDevice
                    {
                        Device = bluetoothDevice,
                        Client = new BluetoothClient()
                    });
                }
            }

            return devices;
        }
    }
}
