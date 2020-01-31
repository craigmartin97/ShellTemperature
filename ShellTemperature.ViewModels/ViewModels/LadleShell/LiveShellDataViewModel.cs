using BluetoothService.BluetoothServices;
using BluetoothService.cs.BluetoothServices;
using Microsoft.Extensions.Configuration;
using OxyPlot;
using OxyPlot.Axes;
using ShellTemperature.Models;
using ShellTemperature.Repository;
using ShellTemperature.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;

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

        /// <summary>
        /// Configuration file
        /// </summary>
        private Dictionary<string, string> _deviceDictionary;
        #endregion

        #region Properties
        private bool _isTimerEnabled = false;
        /// <summary>
        /// Boolean value expressing if the start button is enabled or disabled.
        /// Also, this indicates if the timer is runnig or stopped
        /// </summary>
        public bool IsTimerEnabled
        {
            get => _isTimerEnabled;
            private set
            {
                _isTimerEnabled = value;
                OnPropertyChanged(nameof(IsTimerEnabled));
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
            IConfiguration configuration)
        {
            _shellRepo = repository;

            //get the devices section from the cofig settings
            IEnumerable<IConfigurationSection> configDevices = configuration.GetSection("Devices").GetChildren();
            // covert the devices to a dictionary
            _deviceDictionary = configDevices.ToDictionary(
                dev => dev.Key, dev => dev.Value);

            List<BluetoothDevice> devices = bluetoothFinder.GetBluetoothDevices();

            foreach (BluetoothDevice device in devices)
            {

                bool gotDeviceName = _deviceDictionary.TryGetValue(
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

                dev.Timer.Tick += (sender, args) => Timer_Tick(sender, args, dev);
                dev.Timer.Interval = new TimeSpan(0, 0, 1);
                dev.Timer.Start();

                Devices.Add(dev);
            }

            SelectedDevice = Devices[0];
        }
        #endregion

        #region Timer Ticker
        /// <summary>
        /// Timer_tick executes the start command and then retrieves the bluetooth data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e, Device device)
        {
            try
            {
                double? recievedData = device.BluetoothService.ReadData(device.BluetoothDevice);

                if (recievedData != null)
                {
                    device.CurrentData = device.BluetoothService.GetBluetoothData();
                    ShellTemp shellTemp = new ShellTemp
                    {
                        Temperature = device.CurrentData,
                        RecordedDateTime = DateTime.Now
                    };

                    device.Temp.Add(shellTemp);
                    device.DataPoints.Add((new DataPoint(DateTimeAxis.ToDouble(shellTemp.RecordedDateTime), shellTemp.Temperature)));

                    _shellRepo.Create(shellTemp); // create a new record in the database.
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("An expcetion occurred");
                if (device != null)
                {
                    device.Timer.Stop();
                    Devices.Remove(device);
                }
            }
        }
        #endregion
    }
}
