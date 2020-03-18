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
            catch (IndexOutOfRangeException ex)
            {
                Debug.WriteLine("The index extracting the data was out of range");
                throw;
            }
            catch (SleepException ex)
            {
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
            // client is connected
            NetworkStream stream = device.Client.GetStream();

            if (stream.CanRead)
            {
                do
                {
                    stream.ReadTimeout = 2500; // 500 millisecond timeout, if unable to get data feed
                    int numberOfBytesRead = stream.Read(_myReadBuffer, 0, _myReadBuffer.Length);
                    if (numberOfBytesRead <= 1)
                        continue;

                    string sensorTempValue = Encoding.ASCII.GetString(_myReadBuffer, 0, numberOfBytesRead);
                    if (string.IsNullOrWhiteSpace(sensorTempValue))
                        return null;

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

                    if (latestData.Length == 1) // only one value in the array suspect stoppage
                    {
                        if (latestData[0].Equals("-sleep", StringComparison.CurrentCultureIgnoreCase))
                        {
                            throw new SleepException("The device is now sleeping");
                        }
                    }

                    int sdCardIndex = Array.IndexOf(latestData, "-sdCardData");
                    if (sdCardIndex > -1) // has sd card data
                    {
                        // loop through elements after sd card data element
                        SdCardDeviceReading sdCardData = ExtractBluetoothData(latestData, sdCardIndex + 1);
                        deviceReading.SdCardDeviceReading = sdCardData;
                    }

                    LiveDeviceReading liveDeviceReading = ExtractBluetoothData(latestData);
                    deviceReading.LiveDeviceReading = liveDeviceReading;

                    return deviceReading;
                }
                while (stream.DataAvailable); // only continue if there is more to stream and the parse was successful.

                return null;
            }

            return null;
        }

        public DeviceReading ConnectToDevice(BluetoothDevice device)
        {
            device.Client.SetPin(device.Configuration.Pin); // Set the pin access for the device to auto connect
            device.Client.Connect(device.Device.DeviceAddress,
                InTheHand.Net.Bluetooth.BluetoothService.SerialPort);

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

        #region Extract Data

        private LiveDeviceReading ExtractBluetoothData(string[] latestData)
        {
            LiveDeviceReading liveDeviceReading = new LiveDeviceReading();

            for (int i = 0; i < latestData.Length - 1; i++)
            {
                // has next element
                if (i + 1 > latestData.Length)
                    break;

                string data = latestData[i].Trim();
                // sometimes its stupid and trims the leading T off :/
                if (data.Equals("-temp", StringComparison.CurrentCultureIgnoreCase))
                {
                    bool isDouble = double.TryParse(latestData[i + 1], out double temp);
                    if (isDouble)
                        liveDeviceReading.Temperature = temp;
                }
                else if (data.Equals("-datetime", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (i + 2 > latestData.Length)
                        break;

                    string stringDateTime = latestData[i + 1] + " " + latestData[i + 2];
                    bool isDateTime = DateTime.TryParse(stringDateTime, out DateTime dateTime);
                    if (isDateTime)
                        liveDeviceReading.RecordedDateTime = dateTime;
                }
                else if (data.Equals("-lat", StringComparison.CurrentCultureIgnoreCase))
                {
                    bool isFloat = float.TryParse(latestData[i + 1], out float latitude);
                    if (isFloat)
                        liveDeviceReading.Latitude = latitude;
                }
                else if (data.Equals("-long", StringComparison.CurrentCultureIgnoreCase))
                {
                    bool isFloat = float.TryParse(latestData[i + 1], out float longitude);
                    if (isFloat)
                        liveDeviceReading.Longitude = longitude;
                }
                // Hit the sd card tag so stop as anything after is o valid
                else if (data.Equals("-sdCardData", StringComparison.CurrentCultureIgnoreCase))
                    break;
            }

            // no date time then set to current
            if (liveDeviceReading.RecordedDateTime == null)
                liveDeviceReading.RecordedDateTime = DateTime.Now;

            return liveDeviceReading;
        }

        private SdCardDeviceReading ExtractBluetoothData(string[] latestData, int index)
        {
            SdCardDeviceReading liveDeviceReading = new SdCardDeviceReading();

            for (int i = index; i < latestData.Length - 1; i++)
            {
                // has next element
                if (i + 1 > latestData.Length)
                    break;

                string data = latestData[i].Trim();
                // sometimes its stupid and trims the leading T off :/
                if (data.Equals("-temp", StringComparison.CurrentCultureIgnoreCase))
                {
                    bool isDouble = double.TryParse(latestData[i + 1], out double temp);
                    if (isDouble)
                        liveDeviceReading.Temperature = temp;
                }
                else if (data.Equals("-datetime", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (i + 2 > latestData.Length)
                        break;

                    string stringDateTime = latestData[i + 1] + " " + latestData[i + 2];
                    bool isDateTime = DateTime.TryParse(stringDateTime, out DateTime dateTime);
                    if (isDateTime)
                        liveDeviceReading.RecordedDateTime = dateTime;
                }
                else if (data.Equals("-lat", StringComparison.CurrentCultureIgnoreCase))
                {
                    bool isFloat = float.TryParse(latestData[i + 1], out float latitude);
                    if (isFloat)
                        liveDeviceReading.Latitude = latitude;
                }
                else if (data.Equals("-long", StringComparison.CurrentCultureIgnoreCase))
                {
                    bool isFloat = float.TryParse(latestData[i + 1], out float longitude);
                    if (isFloat)
                        liveDeviceReading.Longitude = longitude;
                }
            }

            return liveDeviceReading;
        }
        #endregion
    }
}