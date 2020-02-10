using BluetoothService.BluetoothServices;
using BluetoothService.cs.BluetoothServices;
using BluetoothService.Enums;
using BluetoothService.Models;
using CustomDialog.Interfaces;
using InTheHand.Net.Sockets;
using Microsoft.Extensions.Configuration;
using OxyPlot;
using OxyPlot.Axes;
using ShellTemperature.Models;
using ShellTemperature.Repository;
using ShellTemperature.ViewModels.Commands;
using ShellTemperature.ViewModels.ConnectionObserver;
using ShellTemperature.ViewModels.TemperatureObserver;
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
        /// The device repository to interact with the data store
        /// </summary>
        private readonly IDeviceRepository<DeviceInfo> _deviceRepository;

        /// <summary>
        /// The subject observer pattern updater
        /// </summary>
        private readonly BluetoothConnectionSubject _subject;

        /// <summary>
        /// Temperature subject to update observers (observer pattern)
        /// </summary>
        private readonly TemperatureSubject _temperatureSubject;

        /// <summary>
        /// Bluetooth foundDevices finder
        /// </summary>
        private readonly IBluetoothFinder _bluetoothFinder;

        private readonly IDialogService _service;

        private readonly Dictionary<string, string> _deviceDictionary;

        private IEnumerable<DeviceInfo> _datastoreDevices;
        #endregion

        #region Properties
        private ObservableCollection<Device> devices = new ObservableCollection<Device>();
        /// <summary>
        /// A collection of devices that bluetooth data can be retrieved from
        /// </summary>
        public ObservableCollection<Device> Devices
        {
            get => devices;
            set
            {
                devices = value;
                OnPropertyChanged(nameof(Devices));
            }
        }

        private Device _selectedFoundDevices;
        /// <summary>
        /// The selected foundDevices from the list.
        /// </summary>
        public Device SelectedDevice
        {
            get => _selectedFoundDevices;
            set
            {
                _selectedFoundDevices = value;
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
            SelectedDevice.IsTimerEnabled = false; // disable the foundDevices start button as already running.
        });

        /// <summary>
        /// Stop command stops the timer from running and stops the execution
        /// of reading data from the bluetooth foundDevices.
        /// </summary>
        public RelayCommand StopCommand
        => new RelayCommand(param =>
        {
            SelectedDevice.Timer.Stop(); // stop the current selected timer.
            SelectedDevice.IsTimerEnabled = true; // enable the start button for the timer

            // can't change connection status if the foundDevices has failed to connect.
            if (SelectedDevice.IsConnected.Equals(DeviceConnectionStatus.FAILED)) return;

            SetConnectionStatus(SelectedDevice, DeviceConnectionStatus.PAUSED);

            //TODO: Need to create a new one as well for display a list of devices that have been found.
            //TODO: Sometimes get bad values Date and Times
        });

        public RelayCommand SearchForDevices
        => new RelayCommand(param =>
        {
            Thread thread = new Thread(() =>
            {
                IList<BluetoothDevice> allDevicesFound = _bluetoothFinder.GetBluetoothDevices();

                for (int i = 0; i < allDevicesFound.Count; i++) // each foundDevices found
                {
                    for (int j = 0; j < Devices.Count; j++) // each foundDevices already found
                    {
                        if (Devices[j].BluetoothDevice.Device
                            .Equals(allDevicesFound[i].Device.DeviceAddress)) // the foundDevices already exists
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
                    Application.Current.Dispatcher?.Invoke(() =>
                    {
                        dev.Timer = new DispatcherTimer();
                        dev.Timer.Tick += (sender, args) => Timer_Tick(dev);
                        dev.Timer.Interval = new TimeSpan(0, 0, 1);
                        dev.Timer.Start();

                        CheckIfDeviceExistsAndCreate(dev);
                        Devices.Add(dev);
                    });
                }

                // set the datastore devices as new onces could have been added.
                _datastoreDevices = _deviceRepository.GetAll();
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
        public LiveShellDataViewModel(IBluetoothFinder bluetoothFinder,
            IRepository<ShellTemp> repository, IDeviceRepository<DeviceInfo> deviceRepository,
            IConfiguration configuration, BluetoothConnectionSubject subject,
            TemperatureSubject temperatureSubject,
            IDialogService service)
        {
            _bluetoothFinder = bluetoothFinder;
            _shellRepo = repository;
            _deviceRepository = deviceRepository;
            _subject = subject;
            _temperatureSubject = temperatureSubject;
            _service = service;

            //get the devices section from the config settings
            IEnumerable<IConfigurationSection> configDevices = configuration
                .GetSection("Devices").GetChildren();

            // covert the devices to a dictionary
            _deviceDictionary = configDevices.ToDictionary(
               dev => dev.Key, dev => dev.Value);

            List<BluetoothDevice> bluetoothDevices = bluetoothFinder.GetBluetoothDevices();

            foreach (BluetoothDevice device in bluetoothDevices)
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

                // check if device exists in the data store
                CheckIfDeviceExistsAndCreate(dev);

                Devices.Add(dev);
            }

            if (Devices.Count == 0) return;

            Devices.OrderBy(x => x.DeviceName);
            SelectedDevice = Devices[0];

            // get all the devices from the data store, this is needed
            _datastoreDevices = _deviceRepository.GetAll();
        }
        #endregion

        #region Timer Ticker

        /// <summary>
        /// Timer_tick executes the start command and then retrieves the bluetooth data.
        /// </summary>
        private void Timer_Tick(Device foundDevices)
        {
            try
            {
                DeviceReading receivedData = foundDevices.BluetoothService.ReadData(foundDevices.BluetoothDevice);

                if (receivedData == null)
                    throw new NullReferenceException("The sensor returned a null response");

                // first three readings ignore as they could be bad, often they are
                if (foundDevices.ReadingsCounter <= 3 && SelectedDevice == foundDevices)
                {
                    Debug.WriteLine("Connecting FoundDevices - " + foundDevices.DeviceName);
                    foundDevices.ReadingsCounter++;

                    SetConnectionStatus(foundDevices, DeviceConnectionStatus.CONNECTING);
                    return;
                }

                // is the date and time recorded valid?
                if (!IsDateTimeValid(receivedData.RecordedDateTime))
                    throw new InvalidOperationException("Invalid Date & Time, Try and Reset the DateTime Module - " +
                                                        foundDevices.DeviceName);

                foundDevices.CurrentData = receivedData.Temperature;

                // get the device from the datastore collection
                DeviceInfo device = _datastoreDevices.FirstOrDefault(x =>
                    x.DeviceAddress.Equals(foundDevices.BluetoothDevice.Device.DeviceAddress.ToString()));

                // create new shell obj for database submission
                ShellTemp shellTemp = new ShellTemp
                {
                    Temperature = receivedData.Temperature,
                    RecordedDateTime = receivedData.RecordedDateTime,
                    Device = device
                };

                foundDevices.Temp.Add(shellTemp);
                foundDevices.DataPoints.Add((new DataPoint(DateTimeAxis.ToDouble(shellTemp.RecordedDateTime),
                    shellTemp.Temperature)));

                _shellRepo.Create(shellTemp); // create a new record in the database.
                _temperatureSubject.SetState(shellTemp);
                SetConnectionStatus(foundDevices, DeviceConnectionStatus.CONNECTED);
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine(ex.Message);

                if (foundDevices != null && SelectedDevice == foundDevices)
                {
                    SetConnectionStatus(foundDevices, DeviceConnectionStatus.CONNECTING);
                    ResetDeviceCounter(foundDevices); // maybe this needs to be outside this if???
                    ResetBluetoothClient(foundDevices);
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message);

                if (foundDevices != null && SelectedDevice == foundDevices)
                {
                    StopCommand.Execute(null);
                    SetConnectionStatus(foundDevices, DeviceConnectionStatus.FAILED, ex.Message);
                    ResetDeviceCounter(foundDevices);
                    ResetBluetoothClient(foundDevices);
                }
            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);

                if (foundDevices != null && SelectedDevice == foundDevices)
                {
                    StopCommand.Execute(null);
                    SetConnectionStatus(foundDevices, DeviceConnectionStatus.FAILED);
                    ResetDeviceCounter(foundDevices);
                    ResetBluetoothClient(foundDevices);
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
        /// <param name="foundDevices">The devices connection to reset</param>
        private void ResetBluetoothClient(Device foundDevices)
        {
            // reset foundDevices
            foundDevices.BluetoothDevice.Client.Close();
            foundDevices.BluetoothDevice.Client = new BluetoothClient();
        }

        /// <summary>
        /// Reset the foundDevices counter
        /// </summary>
        /// <param name="foundDevices">The devices counter to reset</param>
        private void ResetDeviceCounter(Device foundDevices)
        {
            if (foundDevices.ReadingsCounter > 3)
            {
                foundDevices.ReadingsCounter = 0;
            }
        }

        /// <summary>
        /// Change the connection status for the foundDevices
        /// </summary>
        /// <param name="foundDevices">The devices connection status to change</param>
        /// <param name="status">The status to check and change to</param>
        private void SetConnectionStatus(Device foundDevice, DeviceConnectionStatus status)
        {
            if (!foundDevice.IsConnected.Equals(status) && SelectedDevice == foundDevice)
            {
                foundDevice.IsConnected = status;
                SetDeviceState(foundDevice);
            }
        }

        /// <summary>
        /// Change the connection status for the foundDevices
        /// </summary>
        /// <param name="foundDevices">The devices connection status to change</param>
        /// <param name="status">The status to check and change to</param>
        private void SetConnectionStatus(Device foundDevice, DeviceConnectionStatus status, string message)
        {
            if (!foundDevice.IsConnected.Equals(status) && SelectedDevice == foundDevice)
            {
                foundDevice.IsConnected = status;
                SetDeviceState(foundDevice, message);
            }
        }

        /// <summary>
        /// Update the connection status
        /// </summary>
        /// <param name="foundDevices">The foundDevices's connection status to update</param>
        private void SetDeviceState(Device foundDevice) 
            => _subject.SetState(foundDevice);

        /// <summary>
        /// Update the connection status
        /// </summary>
        /// <param name="foundDevices">The foundDevices's connection status to update</param>
        private void SetDeviceState(Device foundDevice, string message)
            => _subject.SetState(foundDevice, message);

        /// <summary>
        /// Get the value from the foundDevices name dictionary
        /// </summary>
        /// <param name="device">FoundDevices to get name from</param>
        /// <param name="deviceName">The foundDevices name to be returned</param>
        /// <returns></returns>
        private string GetValueFromDeviceNameDictionary(BluetoothDevice device)
        {
            bool isString = _deviceDictionary.TryGetValue(
                device.Device.DeviceAddress.ToString(), out string tempDeviceName);

            return isString ? tempDeviceName : device.Device.DeviceAddress.ToString();
        }

        /// <summary>
        /// Check if the device already exists in the database.
        /// If the device, doesn't exist then create the device
        /// </summary>
        /// <param name="device">The device to check if it exists</param>
        private void CheckIfDeviceExistsAndCreate(Device device)
        {
            // check if the device exists
            string deviceAddress = device.BluetoothDevice.Device.DeviceAddress.ToString();
            DeviceInfo dataStoreDevice = _deviceRepository.GetDevice(deviceAddress);

            // the device doesn't exist
            if (dataStoreDevice == null)
            {
                _deviceRepository.Create(new DeviceInfo
                {
                    DeviceName = device.DeviceName,
                    DeviceAddress = deviceAddress
                });
            }
        }

        /// <summary>
        /// Validate that the date and time is between two ranges
        /// </summary>
        /// <param name="currentDateTime">The date and time to validate</param>
        /// <returns>Returns true if the datetime is valid</returns>
        private bool IsDateTimeValid(DateTime currentDateTime)
        {
            // problem with setting time with DS3231 module as gets time at compile 
            DateTime minDateTimeRange = DateTime.Now.AddMinutes(-5);
            DateTime maxDateTimeRange = DateTime.Now.AddMinutes(5);

            // current datetime is only valid if its between the two ranges
            return minDateTimeRange < currentDateTime && maxDateTimeRange > currentDateTime;
        }
        #endregion
    }
}
