using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ShellTemperature.ViewModels.BluetoothServices
{
    public class ReceiverBluetoothService : IReceiverBluetoothService, IDisposable
    {
        private BluetoothListener _listener;
        private CancellationTokenSource _cancelSource;
        private BluetoothClient client;

        private string _data;
        public string Data { get => _data; set => _data = value; }

        /// <summary>  
        /// Starts the listening from Senders.  
        /// </summary>  
        /// <param name="reportAction">  
        /// The report Action.  
        /// </param>  
        public void Start()
        {
            Listener();
        }

        /// <summary>  
        /// Stops the listening from Senders.  
        /// </summary>  
        public void Stop()
        {
            _cancelSource.Cancel();
        }

        /// <summary>  
        /// Listeners the accept bluetooth client.  
        /// </summary>  
        /// <param name="token">  
        /// The token.  
        /// </param>  
        private void Listener()
        {
            BluetoothClient bc = new BluetoothClient();
            client = bc;
            BluetoothDeviceInfo[] devices = bc.DiscoverDevices(1);
            BluetoothDeviceInfo device = devices[0];

            BluetoothEndPoint endPoint = new BluetoothEndPoint(device.DeviceAddress, BluetoothService.BluetoothBase);

            bc.BeginConnect(device.DeviceAddress, BluetoothService.SerialPort, new AsyncCallback(Connect), device);
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
                    do
                    {
                        int numberOfBytesRead = stream.Read(myReadBuffer, 0, myReadBuffer.Length);
                        myCompleteMessage.AppendFormat("{0}", Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
                    }
                    while (stream.DataAvailable);

                    Data = myCompleteMessage.ToString();
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
    }
}
