using BluetoothService.cs.BluetoothServices;
using BluetoothService.Models;
using InTheHand.Net.Sockets;
using System;
using System.Diagnostics;
using System.IO;
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

        private readonly byte[] _myReadBuffer = new byte[2048];


        /// <summary>  
        /// Starts the listening from Senders.  
        /// </summary>  
        /// <param name="reportAction">  
        /// The report Action.  
        /// </param>
        /// <param name="device"></param>  
        public DeviceReading ReadData(BluetoothDevice device)
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
            catch (IOException ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine("Unable to get network stream");
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

        private DeviceReading Connect(BluetoothDevice device)
        {
            Debug.WriteLine("Getting network stream");
            // client is connected
            NetworkStream stream = device.Client.GetStream();

            if (stream.CanRead)
            {
                do
                {
                    Thread.Sleep(100);
                    stream.ReadTimeout = 1000; // 500 millisecond timeout, if unable to get data feed
                    int numberOfBytesRead = stream.Read(_myReadBuffer, 0, _myReadBuffer.Length);
                    Debug.WriteLine("Got number of bytes: " + numberOfBytesRead);
                    if (numberOfBytesRead <= 1)
                        continue;

                    string sensorTempValue = Encoding.ASCII.GetString(_myReadBuffer, 0, numberOfBytesRead);
                    if (string.IsNullOrWhiteSpace(sensorTempValue))
                        return null;


                    Debug.WriteLine("sensor temp value" + sensorTempValue);
                    string[] arr = sensorTempValue.Split(Environment.NewLine).ToArray();

                    if (arr.Length == 0)
                        return null; // nothing to get

                    DeviceReading deviceReading = new DeviceReading();

                    string latestReading = arr.LastOrDefault(x => !string.IsNullOrWhiteSpace(x));

                    if (string.IsNullOrWhiteSpace(latestReading))
                        return null;

                    string[] latestData = latestReading.Split(' ')
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToArray();

                    bool hasDateTime = false;

                    for (int i = 0; i < latestData.Length - 1; i++)
                    {
                        string data = latestData[i].Trim();
                        // sometimes its stupid and trims the leading T off :/
                        if (data.Equals("-temp", StringComparison.CurrentCultureIgnoreCase))
                        {
                            bool isDouble = double.TryParse(latestData[i + 1], out double temp);
                            if (isDouble)
                                deviceReading.Temperature = temp;
                        }
                        else if (data.Equals("-datetime", StringComparison.CurrentCultureIgnoreCase))
                        {
                            string stringDateTime = latestData[i + 1] + " " + latestData[i + 2];
                            bool isDateTime = DateTime.TryParse(stringDateTime, out DateTime dateTime);
                            if (isDateTime)
                            {
                                deviceReading.RecordedDateTime = dateTime;
                                hasDateTime = true;
                            }
                        }
                        else if (data.Equals("-lat", StringComparison.CurrentCultureIgnoreCase))
                        {
                            bool isFloat = float.TryParse(latestData[i + 1], out float latitude);
                            if (isFloat)
                                deviceReading.Latitude = latitude;
                        }
                        else if (data.Equals("-long", StringComparison.CurrentCultureIgnoreCase))
                        {
                            bool isFloat = float.TryParse(latestData[i + 1], out float longitude);
                            if (isFloat)
                                deviceReading.Longitude = longitude;
                        }
                    }

                    // no date time then set to current
                    if (!hasDateTime)
                        deviceReading.RecordedDateTime = DateTime.Now;

                    return deviceReading;

                    //int indexOfFirstSpace = latestReading.IndexOf(' ');
                    //if (indexOfFirstSpace > 0) // got space
                    //{
                    //    string temp = latestReading.Substring(0, indexOfFirstSpace);
                    //    string dateTime = latestReading.Substring(indexOfFirstSpace);

                    //    if (!double.TryParse(temp, out double tempAsDouble) ||
                    //        !DateTime.TryParse(dateTime, out DateTime dateAsDateTime)) continue;

                    //    Debug.WriteLine("GOT DOUBLE: " + tempAsDouble);
                    //    deviceReading = new DeviceReading
                    //    {
                    //        Temperature = tempAsDouble,
                    //        RecordedDateTime = dateAsDateTime
                    //    };
                    //}
                    //else
                    //{
                    //    bool isDouble = double.TryParse(arr.LastOrDefault(x => !string.IsNullOrWhiteSpace(x)),
                    //        out double d);

                    //    if (!isDouble)
                    //        continue;

                    //    deviceReading = new DeviceReading
                    //    {
                    //        Temperature = d,
                    //        RecordedDateTime = DateTime.Now // no date time read, so just use best next thing :/
                    //    };
                    //}


                }
                while (stream.DataAvailable); // only continue if there is more to stream and the parse was successful.

                stream.Close(); // test
                return null;
            }
            else
            {
                Debug.WriteLine("Sorry.  You cannot read from this NetworkStream.");
                return null;
            }
        }

        public DeviceReading ConnectToDevice(BluetoothDevice device)
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
            if (!disposing) return;
            if (_cancelSource == null) return;

            _listener.Stop();
            _listener = null;
            _cancelSource.Dispose();
            _cancelSource = null;
        }
    }
}