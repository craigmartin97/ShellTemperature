using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ShellTemperature.ViewModels.BluetoothServices
{
    public class ReceiverBluetoothService : IReceiverBluetoothService, IDisposable
    {
        private BluetoothListener _listener;
        private CancellationTokenSource _cancelSource;

        private readonly BluetoothClient client = new BluetoothClient();
        private readonly BluetoothDeviceInfo device;

        /// <summary>
        /// Data that has been read from the bluetooth service.
        /// </summary>
        private double _data;

        public ReceiverBluetoothService()
        {
            BluetoothDeviceInfo[] devices = client.DiscoverDevices(1);

            if ((devices != null) && devices.Count() > 0) // has connected device.
                device = devices[0];
        }

        /// <summary>  
        /// Starts the listening from Senders.  
        /// </summary>  
        /// <param name="reportAction">  
        /// The report Action.  
        /// </param>  
        public void ReadData()
        {
            if (device != null) // ensure the device is not null to contiune with reading data.
            {
                try
                {
                    if (client.Connected) Connect();
                    else client.BeginConnect(device.DeviceAddress, BluetoothService.SerialPort, new AsyncCallback(Connect), device);
                }
                catch (SocketException ex)
                {
                    Debug.WriteLine(ex.Message);
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
        }

        /// <summary>  
        /// Stops the listening from Senders.  
        /// </summary>  
        public void Stop()
        {
            client.Close();
            client.Dispose();
        }

        private void Connect()
        {

            // client is connected
            NetworkStream stream = client.GetStream();

            if (stream.CanRead)
            {
                byte[] myReadBuffer = new byte[1024];
                StringBuilder myCompleteMessage = new StringBuilder();

                // Incoming message may be larger than the buffer size. 
                bool parsedLastVal = false;
                do
                {
                    int numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                    myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

                    parsedLastVal = double.TryParse(myCompleteMessage.ToString().Split(Environment.NewLine.ToCharArray())
                        .Last(x => !string.IsNullOrWhiteSpace(x)), out _data);
                }
                while (stream.DataAvailable && parsedLastVal); // only contiune if there is more to stream and the parse was successful.


                // Print out the received message to the console.
                Debug.WriteLine("You received the following message : " + myCompleteMessage);
            }
            else
            {
                Debug.WriteLine("Sorry.  You cannot read from this NetworkStream.");
            }
        }

        private void Connect(IAsyncResult result)
        {
            if (result.IsCompleted)
            {
                // client is connected
                NetworkStream stream = client.GetStream();

                if (stream.CanRead)
                {
                    byte[] myReadBuffer = new byte[1024];
                    StringBuilder myCompleteMessage = new StringBuilder();

                    // Incoming message may be larger than the buffer size. 
                    bool parsedLastVal = false;
                    do
                    {
                        int numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                        myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));

                        parsedLastVal = double.TryParse(myCompleteMessage.ToString().Split(Environment.NewLine.ToCharArray())
                            .Last(x => !string.IsNullOrWhiteSpace(x)), out _data);
                    }
                    while (stream.DataAvailable && parsedLastVal); // only contiune if there is more to stream and the parse was successful.


                    // Print out the received message to the console.
                    Debug.WriteLine("You received the following message : " + myCompleteMessage);
                }
                else
                {
                    Debug.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                }
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
