using BluetoothService.cs.BluetoothServices;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace BluetoothService.BluetoothServices
{
    public class ReceiverBluetoothService : IReceiverBluetoothService, IDisposable
    {
        private BluetoothListener _listener;
        private CancellationTokenSource _cancelSource;

        private readonly BluetoothClient client = new BluetoothClient();

        private readonly byte[] myReadBuffer = new byte[1024];
        private List<BluetoothDevice> Devices = new List<BluetoothDevice>();

        private readonly string[] temperatureDevices = new string[]
        {
            "DSD TECH HC-05"
        };

        /// <summary>
        /// Data that has been read from the bluetooth service.
        /// </summary>
        private double _data;

        /// <summary>  
        /// Starts the listening from Senders.  
        /// </summary>  
        /// <param name="reportAction">  
        /// The report Action.  
        /// </param>  
        public void ReadData(BluetoothDevice device)
        {
            //if ((Devices != null) && Devices.Count > 0) // ensure the device is not null to contiune with reading data.
            //{
            //    foreach (BluetoothDevice device in Devices)
            //    {
                    try
                    {
                        if (device.Client.Connected) Connect(device);
                        else
                        {
                            device.Client.Connect(device.Device.DeviceAddress, InTheHand.Net.Bluetooth.BluetoothService.SerialPort);
                            //device.Client.BeginConnect(device.Device.DeviceAddress, InTheHand.Net.Bluetooth.BluetoothService.SerialPort, new AsyncCallback(Connect), device.Device);
                        }
                    }
                    catch (SocketException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Debug.WriteLine("A reading from the device has tried to be read and failed, remove the device");
                        Devices.Remove(device);
                    }
                    catch (ObjectDisposedException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Unexpected exception has occurred whilst reading the bluetooth data");
                        Debug.Write(ex.Message);
                        throw;
                    }
            //    }
            //}
        }

        public List<BluetoothDevice> GetBluetoothDevices()
        {
            BluetoothDeviceInfo[] foundDevices = client.DiscoverDevices(1000, true, true, false, true);

            foreach (string s in temperatureDevices)
            {
                List<BluetoothDeviceInfo> matches = foundDevices.Where(x => x.DeviceName.ToLower().Equals(s.ToLower())).ToList();

                foreach (BluetoothDeviceInfo bluetoothDevice in matches)
                {
                    Devices.Add(new BluetoothDevice
                    {
                        Device = bluetoothDevice,
                        Client = new BluetoothClient()
                    });
                }
            }

            return Devices;
        }


        /// <summary>  
        /// Stops the listening from Senders.  
        /// </summary>  
        public void Stop()
        {
            client.Close();
            client.Dispose();
        }

        private void Connect(BluetoothDevice device)
        {
            // client is connected
            NetworkStream stream = device.Client.GetStream();

            if (stream.CanRead)
            {
                do
                {
                    int numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);

                    if (numberOfBytesRead <= 1)
                        continue;

                    var s = Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead);
                    bool succ = double.TryParse(s, out double d);
                    if (!succ)
                        continue;

                    _data = d;
                }
                while (stream.DataAvailable); // only contiune if there is more to stream and the parse was successful.

                // Print out the received message to the console.
                //Debug.WriteLine("You received the following message : " + myCompleteMessage);
            }
            else
            {
                Debug.WriteLine("Sorry.  You cannot read from this NetworkStream.");
            }
        }

        /// <summary>  
        /// The dispose.  
        /// </summary>  
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>  
        /// The dispose.  
        /// </summary>  
        /// <param name="disposing">  
        /// The disposing.  
        /// </param>  
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_cancelSource != null)
                {
                    _listener.Stop();
                    _listener = null;
                    _cancelSource.Dispose();
                    _cancelSource = null;
                }
            }
        }

        /// <summary>
        /// Return the bluetooth data that has been retrieved.
        /// </summary>
        /// <returns>Return string content of bluetooth data that has been retrieved.</returns>
        public double GetBluetoothData() => _data;

        public void ReadData()
        {
            throw new NotImplementedException();
        }
    }
}
