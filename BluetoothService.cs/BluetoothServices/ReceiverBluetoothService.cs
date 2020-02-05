using BluetoothService.cs.BluetoothServices;
using InTheHand.Net.Sockets;
using System;
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

        private readonly BluetoothClient _client = new BluetoothClient();

        private readonly byte[] _myReadBuffer = new byte[1024];


        /// <summary>  
        /// Starts the listening from Senders.  
        /// </summary>  
        /// <param name="reportAction">  
        /// The report Action.  
        /// </param>
        /// <param name="device"></param>  
        public double? ReadData(BluetoothDevice device)
        {
            try
            {
                return device.Client.Connected ? Connect(device) : ConnectToDevice(device);
            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("A reading from the device has tried to be read and failed, remove the device");
                throw;
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
        }

        /// <summary>  
        /// Stops the listening from Senders.  
        /// </summary>  
        public void Stop()
        {
            _client.Close();
            _client.Dispose();
        }

        private double? Connect(BluetoothDevice device)
        {
            // client is connected
            NetworkStream stream = device.Client.GetStream();

            if (stream.CanRead)
            {
                do
                {
                    int numberOfBytesRead = stream.Read(_myReadBuffer, 0, _myReadBuffer.Length);

                    if (numberOfBytesRead <= 1)
                        continue;

                    string sensorTempValue = Encoding.ASCII.GetString(_myReadBuffer, 0, numberOfBytesRead);
                    string[] arr = sensorTempValue.Split(Environment.NewLine).ToArray();
                    bool isDouble = double.TryParse(arr.LastOrDefault(x => !string.IsNullOrWhiteSpace(x)), 
                        out double d);

                    if (!isDouble)
                        continue;

                    return d;
                }
                while (stream.DataAvailable); // only continue if there is more to stream and the parse was successful.

                return null;
            }
            else
            {
                Debug.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                return null;
            }
        }

        public double? ConnectToDevice(BluetoothDevice device)
        {
            device.Client.Connect(device.Device.DeviceAddress,
                InTheHand.Net.Bluetooth.BluetoothService.SerialPort);

            //return null;
            return Connect(device);
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
    }
}
