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
        public double? ReadData(BluetoothDevice device)
        {
            try
            {
                if (device.Client.Connected)
                {
                    return Connect(device);
                } 
                else
                {
                    device.Client.Connect(device.Device.DeviceAddress, InTheHand.Net.Bluetooth.BluetoothService.SerialPort);
                    return null;
                }
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
            client.Close();
            client.Dispose();
        }

        private double? Connect(BluetoothDevice device)
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

                    string sensorTempValue = Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead);
                    bool isDouble = double.TryParse(sensorTempValue, out double d);
                    if (!isDouble)
                        continue;

                    _data = d;
                    return d;
                }
                while (stream.DataAvailable); // only contiune if there is more to stream and the parse was successful.

                return null;
            }
            else
            {
                Debug.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                return null;
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
    }
}
