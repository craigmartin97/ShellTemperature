using BluetoothService.BluetoothServices;
using BluetoothService.cs.BluetoothServices;
using InTheHand.Net.Sockets;
using Microsoft.Extensions.Configuration;
using OxyPlot;
using OxyPlot.Axes;
using ShellTemperature.Models;
using ShellTemperature.Repository;
using ShellTemperature.ViewModels.Commands;
using ShellTemperature.ViewModels.ConnectionObserver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Threading;
using BluetoothService.Enums;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace ShellTemperature.ViewModels.ViewModels.LadleShell
{
    /// <summary>
    /// Live shell data view model is responsible for retrieving the live
    /// temperature data and displaying the results to the user
    /// </summary>
    public class LiveShellDataViewModel : BaseShellViewModel
    {
        #region Private fields

        /// <summary>
        /// Repository implementation of the ShellTemperature that allows to Create, Read, Update and Delete.
        /// </summary>
        private readonly IRepository<ShellTemp> _shellRepo;

        private readonly BluetoothConnectionSubject _subject;
        #endregion

        #region Properties
        private Device _selectedDevice;
        /// <summary>
        /// The selected device from the list.
        /// </summary>
        public sealed override Device SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (value == null) return;

                _selectedDevice = value;
                _subject.SetState(value);
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Start reading the bluetooth service
        /// </summary>
        public RelayCommand StartCommand
        => new RelayCommand(param =>
        {
            SelectedDevice.Timer.Start();
            SelectedDevice.IsTimerEnabled = false; // disable the device start button as already running.
        });

        /// <summary>
        /// Stop command stops the timer from running and stops the execution
        /// of reading data from the bluetooth device.
        /// </summary>
        public RelayCommand StopCommand
        => new RelayCommand(param =>
        {
            SelectedDevice.Timer.Stop(); // stop the current selected timer.
            SelectedDevice.IsTimerEnabled = true; // enable the start button for the timer
        });

        #endregion

        #region Constructors
        public LiveShellDataViewModel(IBluetoothFinder bluetoothFinder, IRepository<ShellTemp> repository,
            IConfiguration configuration, BluetoothConnectionSubject subject)
        {
            _shellRepo = repository;
            _subject = subject;

            //get the devices section from the cofig settings
            IEnumerable<IConfigurationSection> configDevices = configuration.GetSection("Devices").GetChildren();
            // covert the devices to a dictionary
            Dictionary<string, string> deviceDictionary = configDevices.ToDictionary(
                dev => dev.Key, dev => dev.Value);

            List<BluetoothDevice> devices = bluetoothFinder.GetBluetoothDevices();

            foreach (BluetoothDevice device in devices)
            {

                bool gotDeviceName = deviceDictionary.TryGetValue(
                    device.Device.DeviceAddress.ToString(), out string deviceName);

                // if got device name, use that else use the address of device.
                deviceName = gotDeviceName ? deviceName : device.Device.DeviceAddress.ToString();

                Device dev = new Device()
                {
                    Timer = new DispatcherTimer(),
                    CurrentData = 0,
                    DataPoints = new ObservableCollection<DataPoint>(),
                    Temp = new ObservableCollection<ShellTemp>(),
                    BluetoothService = new ReceiverBluetoothService(),
                    BluetoothDevice = device,
                    IsTimerEnabled = false,
                    DeviceName = deviceName
                };

                dev.Timer.Tick += (sender, args) => Timer_Tick(dev);
                dev.Timer.Interval = new TimeSpan(0, 0, 1);
                dev.Timer.Start();

                Devices.Add(dev);
            }

            if (Devices.Count == 0) return;

            Devices.OrderBy(x => x.DeviceName);
            SelectedDevice = Devices[0];
        }
        #endregion

        #region Timer Ticker

        /// <summary>
        /// Timer_tick executes the start command and then retrieves the bluetooth data.
        /// </summary>
        private void Timer_Tick(Device device)
        {
            try
            {
                double? receivedData = device.BluetoothService.ReadData(device.BluetoothDevice);

                if (receivedData == null)
                    throw new NullReferenceException("The sensor returned a null response");

                // first three readings ignore as they could be bad, often they are
                if (device.ReadingsCounter <= 3 && SelectedDevice == device)
                {
                    Debug.WriteLine("Connecting Device - " + device.DeviceName);
                    device.ReadingsCounter++;

                    SetConnectionStatus(device, DeviceConnectionStatus.CONNECTING);
                    return;
                }

                device.CurrentData = (double)receivedData;

                ShellTemp shellTemp = new ShellTemp
                {
                    Temperature = device.CurrentData,
                    RecordedDateTime = DateTime.Now
                };

                device.Temp.Add(shellTemp);
                device.DataPoints.Add((new DataPoint(DateTimeAxis.ToDouble(shellTemp.RecordedDateTime),
                    shellTemp.Temperature)));

                _shellRepo.Create(shellTemp); // create a new record in the database.
                SetConnectionStatus(device, DeviceConnectionStatus.CONNECTED);
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine(ex.Message);

                if (device != null && SelectedDevice == device)
                {
                    SetConnectionStatus(device, DeviceConnectionStatus.FAILED);
                    ResetDeviceCounter(device); // maybe this needs to be outside this if???
                    ResetBluetoothClient(device);
                }
            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);

                if (device != null && SelectedDevice == device)
                {
                    StopCommand.Execute(null);
                    SetConnectionStatus(device, DeviceConnectionStatus.FAILED);
                    ResetDeviceCounter(device);
                    ResetBluetoothClient(device);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An exception occurred");
                Debug.WriteLine(ex.Message);
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Close the bluetooth connection.
        /// Reset the connection object to be able to reconnect later.
        /// </summary>
        /// <param name="device">The devices connection to reset</param>
        private void ResetBluetoothClient(Device device)
        {
            // reset device
            device.BluetoothDevice.Client.Close();
            device.BluetoothDevice.Client = new BluetoothClient();
        }

        /// <summary>
        /// Reset the device counter
        /// </summary>
        /// <param name="device">The devices counter to reset</param>
        private void ResetDeviceCounter(Device device)
        {
            if (device.ReadingsCounter > 3)
            {
                device.ReadingsCounter = 0;
            }
        }

        /// <summary>
        /// Change the connection status for the device
        /// </summary>
        /// <param name="device">The devices connection status to change</param>
        /// <param name="status">The status to check and change to</param>
        private void SetConnectionStatus(Device device, DeviceConnectionStatus status)
        {
            if (!device.IsConnected.Equals(status) && SelectedDevice == device)
            {
                device.IsConnected = status;
                SetDeviceState(device);
            }
        }

        /// <summary>
        /// Update the connection status
        /// </summary>
        /// <param name="device">The device's connection status to update</param>
        private void SetDeviceState(Device device) => _subject.SetState(device);

        #endregion
    }
}
