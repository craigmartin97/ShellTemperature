using BluetoothService.BluetoothServices;
using BluetoothService.cs.BluetoothServices;
using BluetoothService.Enums;
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
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
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
        /// The subject observer pattern updater
        /// </summary>
        private readonly BluetoothConnectionSubject _subject;

        /// <summary>
        /// Bluetooth device finder
        /// </summary>
        private readonly IBluetoothFinder _bluetoothFinder;

        private readonly Dictionary<string, string> _deviceDictionary;
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

            // can't change connection status if the device has failed to connect.
            if (SelectedDevice.IsConnected.Equals(DeviceConnectionStatus.FAILED)) return;

            SetConnectionStatus(SelectedDevice, DeviceConnectionStatus.PAUSED);
        });

        public RelayCommand SearchForDevices
        => new RelayCommand(param =>
        {
            Thread thread = new Thread(() =>
            {
                IList<BluetoothDevice> allDevicesFound = _bluetoothFinder.GetBluetoothDevices();

                for (int i = 0; i < allDevicesFound.Count; i++) // each device found
                {
                    for (int j = 0; j < Devices.Count; j++) // each device already found
                    {
                        if (Devices[j].BluetoothDevice.Device
                            .Equals(allDevicesFound[i].Device.DeviceAddress)) // the device already exists
                        {
                            allDevicesFound.Remove(allDevicesFound[i]);
                        }
                    }
                }

                foreach (BluetoothDevice device in allDevicesFound)
                {
                    Device dev = new Device
                    {
                        CurrentData = 0,
                        DataPoints = new ObservableCollection<DataPoint>(),
                        Temp = new ObservableCollection<ShellTemp>(),
                        BluetoothService = new ReceiverBluetoothService(),
                        BluetoothDevice = device,
                        IsTimerEnabled = false,
                        DeviceName = GetValueFromDeviceNameDictionary(device)
                    };

                    // add devices back on main thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        dev.Timer = new DispatcherTimer();
                        dev.Timer.Tick += (sender, args) => Timer_Tick(dev);
                        dev.Timer.Interval = new TimeSpan(0, 0, 1);
                        dev.Timer.Start();

                        Devices.Add(dev);
                    });
                }
            });

            thread.Start();
        });

        public RelayCommand RemoveSelectedDevice =>
        new RelayCommand(param =>
        {
            if (SelectedDevice == null || Devices.Count == 0) return;

            SelectedDevice.Timer.Stop();
            SelectedDevice.BluetoothDevice.Client.Close();
            Devices.Remove(SelectedDevice);

            SelectedDevice = Devices.FirstOrDefault(); // reset to the next item
        });
        #endregion

        #region Constructors
        public LiveShellDataViewModel(IBluetoothFinder bluetoothFinder, IRepository<ShellTemp> repository,
            IConfiguration configuration, BluetoothConnectionSubject subject)
        {
            _bluetoothFinder = bluetoothFinder;
            _shellRepo = repository;
            _subject = subject;

            //get the devices section from the config settings
            IEnumerable<IConfigurationSection> configDevices = configuration
                .GetSection("Devices").GetChildren();

            // covert the devices to a dictionary
            _deviceDictionary = configDevices.ToDictionary(
               dev => dev.Key, dev => dev.Value);

            List<BluetoothDevice> devices = bluetoothFinder.GetBluetoothDevices();

            foreach (BluetoothDevice device in devices)
            {
                Device dev = new Device()
                {
                    Timer = new DispatcherTimer(),
                    CurrentData = 0,
                    DataPoints = new ObservableCollection<DataPoint>(),
                    Temp = new ObservableCollection<ShellTemp>(),
                    BluetoothService = new ReceiverBluetoothService(),
                    BluetoothDevice = device,
                    IsTimerEnabled = false,
                    DeviceName = GetValueFromDeviceNameDictionary(device)
                };
                // set to connecting, in the constructor its being setup so its a connecting status
                SetConnectionStatus(dev, DeviceConnectionStatus.CONNECTING);

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

        /// <summary>
        /// Get the value from the device name dictionary
        /// </summary>
        /// <param name="device">Device to get name from</param>
        /// <param name="deviceName">The device name to be returned</param>
        /// <returns></returns>
        private string GetValueFromDeviceNameDictionary(BluetoothDevice device)
        {
            bool isString = _deviceDictionary.TryGetValue(
                device.Device.DeviceAddress.ToString(), out string tempDeviceName);

            return isString ? tempDeviceName : device.Device.DeviceAddress.ToString();
        }
        #endregion
    }
}
