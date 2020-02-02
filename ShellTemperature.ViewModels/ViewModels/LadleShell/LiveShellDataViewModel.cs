using BluetoothService.BluetoothServices;
using BluetoothService.cs.BluetoothServices;
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
using System.Diagnostics;
using System.Linq;
using System.Windows.Threading;
using InTheHand.Net.Sockets;

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
        private readonly Dictionary<string, string> _deviceDictionary;

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

            if (Devices.Count == 0) return;

            Devices.OrderBy(x => x.DeviceName);
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
                double? receivedData = device.BluetoothService.ReadData(device.BluetoothDevice);
                    //device.IsRunning 
                    //? device.BluetoothService.ReadData(device.BluetoothDevice) 
                    //: device.BluetoothService.ConnectToDevice(device.BluetoothDevice);

                if (receivedData == null)
                    throw new NullReferenceException("The sensor returned a null response");

                device.CurrentData = device.BluetoothService.GetBluetoothData();
                ShellTemp shellTemp = new ShellTemp
                {
                    Temperature = device.CurrentData,
                    RecordedDateTime = DateTime.Now
                };

                device.Temp.Add(shellTemp);
                device.DataPoints.Add((new DataPoint(DateTimeAxis.ToDouble(shellTemp.RecordedDateTime),
                    shellTemp.Temperature)));

                _shellRepo.Create(shellTemp); // create a new record in the database.

                if (!device.IsRunning && SelectedDevice == device) // the device is not running and the selected device is the current device
                {
                    device.IsRunning = true;
                    _subject.SetState(device); // update the device
                }
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine(ex.Message);
                if (device != null)
                {
                    if (SelectedDevice == device)
                    {
                        device.IsRunning = false;
                        _subject.SetState(device);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An expcetion occurred");
            }
        }
        #endregion
    }
}
