using BluetoothService.BluetoothServices;
using BluetoothService.cs.BluetoothServices;
using BluetoothService.Enums;
using BluetoothService.Models;
using CustomDialog.Interfaces;
using InTheHand.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OxyPlot;
using OxyPlot.Axes;
using ShellTemperature.Models;
using ShellTemperature.Repository;
using ShellTemperature.ViewModels.Commands;
using ShellTemperature.ViewModels.ConnectionObserver;
using ShellTemperature.ViewModels.Outliers;
using ShellTemperature.ViewModels.TemperatureObserver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using BluetoothService;
using CustomDialog.Dialogs;
using CustomDialog.Enums;
using CustomDialog.Services;

namespace ShellTemperature.ViewModels.ViewModels.LadleShell
{
    /// <summary>
    /// Live shell data view model is responsible for retrieving the live
    /// temperature data and displaying the results to the user
    /// </summary>
    public class LiveShellDataViewModel : ViewModelBase
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

        /// <summary>
        /// Dialog service to display info boxes to the user
        /// </summary>
        private readonly IDialogService _service;

        /// <summary>
        /// Dictionary of all the devices and there recognizable names
        /// </summary>
        private readonly Dictionary<string, string> _deviceDictionary;

        /// <summary>
        /// Collection of all the devices from the database
        /// </summary>
        private IEnumerable<DeviceInfo> _datastoreDevices;

        /// <summary>
        /// Outlier detector to determined if the current value is an outlier.
        /// </summary>
        private readonly OutlierDetector _outlierDetector = new OutlierDetector();

        /// <summary>
        /// Logger to record system messages
        /// </summary>
        private readonly ILogger<LiveShellDataViewModel> _logger;
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

        private Device _selectedDevice;
        /// <summary>
        /// The selected foundDevices from the list.
        /// </summary>
        public Device SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if(value == null)
                    return;

                _selectedDevice = value;
                _subject.SetState(value.State);

                OnPropertyChanged(nameof(SelectedDevice));
            }
        }

        private bool _isSearchForDeviceEnabled = true;
        /// <summary>
        /// Is the search for devices functionality enabled or disabled
        /// (Default enabled)
        /// </summary>
        public bool IsSearchForDevicesEnabled
        {
            get => _isSearchForDeviceEnabled;
            set
            {
                _isSearchForDeviceEnabled = value;
                OnPropertyChanged(nameof(IsSearchForDevicesEnabled));
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
            if (SelectedDevice.State.IsConnected.Equals(DeviceConnectionStatus.FAILED)) return;

            SelectedDevice.State.Message = BLTError.paused + SelectedDevice.DeviceName;
            SetConnectionStatus(SelectedDevice, DeviceConnectionStatus.PAUSED);
        });

        public RelayCommand SearchForDevices
        => new RelayCommand(param =>
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    // disable the search functionality
                    Application.Current.Dispatcher?.Invoke(() => IsSearchForDevicesEnabled = false);

                    IList<BluetoothDevice> allDevicesFound = _bluetoothFinder.GetBluetoothDevices();

                    for (int i = 0; i < allDevicesFound.Count; i++) // each foundDevices found
                    {
                        for (int j = 0; j < Devices.Count; j++) // each foundDevices already found
                        {
                            if (Devices[j].BluetoothDevice.Device.DeviceAddress
                                .Equals(allDevicesFound[i].Device.DeviceAddress)) // the foundDevices already exists
                            {
                                allDevicesFound.Remove(allDevicesFound[i]);
                            }
                        }
                    }

                    if (allDevicesFound.Count == 0)
                    {
                        return;
                    }

                    DialogResult result = DialogResult.Undefined;
                    // ask user if they want to start recording data from new devices
                    Application.Current.Dispatcher?.Invoke(() =>
                    {
                        DialogService service = new DialogService();
                        ConfirmationDialogViewModel confirmation = new ConfirmationDialogViewModel
                            ("Found New Devices",
                            "Found " + allDevicesFound.Count + " device(s). Would you like to start recording data?");
                        result = service.OpenDialogService(confirmation);
                    });

                    // the user did not press yes.
                    if (result == DialogResult.Undefined || result == DialogResult.No)
                        return;

                    // user pressed yes so add devices
                    foreach (BluetoothDevice device in allDevicesFound)
                    {
                        Device dev = new Device(device, GetValueFromDeviceNameDictionary(device));

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
                }
                finally
                {
                    Application.Current.Dispatcher?.Invoke(() => IsSearchForDevicesEnabled = true);
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
        public LiveShellDataViewModel(IBluetoothFinder bluetoothFinder,
            IRepository<ShellTemp> repository, IDeviceRepository<DeviceInfo> deviceRepository,
            IConfiguration configuration, BluetoothConnectionSubject subject,
            TemperatureSubject temperatureSubject,
            IDialogService service,
            ILogger<LiveShellDataViewModel> logger)
        {
            _bluetoothFinder = bluetoothFinder;
            _shellRepo = repository;
            _deviceRepository = deviceRepository;
            _subject = subject;
            _temperatureSubject = temperatureSubject;
            _service = service;
            _logger = logger;

            //get the devices section from the config settings
            IEnumerable<IConfigurationSection> configDevices = configuration
                .GetSection("Devices").GetChildren();

            // covert the devices to a dictionary
            _deviceDictionary = configDevices.ToDictionary(
               dev => dev.Key, dev => dev.Value);

            List<BluetoothDevice> bluetoothDevices = bluetoothFinder.GetBluetoothDevices();

            foreach (BluetoothDevice device in bluetoothDevices)
            {
                Device dev = new Device(device, GetValueFromDeviceNameDictionary(device));

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
        private void Timer_Tick(Device currentDevice)
        {
            try
            {
                DeviceReading receivedData = currentDevice.BluetoothService.ReadData(currentDevice.BluetoothDevice);

                if (receivedData == null)
                    throw new NullReferenceException("The sensor returned a null response");

                // is the date and time recorded valid?
                if (!IsDateTimeValid(receivedData.RecordedDateTime))
                    throw new InvalidOperationException("Invalid Date & Time, Try and Reset the DateTime Module - " +
                                                        currentDevice.DeviceName);

                currentDevice.AllTemperatureReadings.Add(receivedData.Temperature);

                // check if outlier
                bool isOutlier = _outlierDetector.IsOutlier(receivedData.Temperature,
                    5, currentDevice.AllTemperatureReadings);

                if (isOutlier) // value is outlier cannot be added
                    return;

                // first three readings ignore as they could be bad, often they are
                if (currentDevice.ReadingsCounter <= 3 && SelectedDevice == currentDevice)
                {
                    Debug.WriteLine("Connecting FoundDevices - " + currentDevice.DeviceName);
                    currentDevice.ReadingsCounter++;

                    currentDevice.State.Message = BLTError.connecting + currentDevice.DeviceName;
                    SetConnectionStatus(currentDevice, DeviceConnectionStatus.CONNECTING);
                    return;
                }


                currentDevice.CurrentData = receivedData.Temperature;

                // get the device from the datastore collection
                DeviceInfo device = _datastoreDevices.FirstOrDefault(x =>
                    x.DeviceAddress.Equals(currentDevice.BluetoothDevice.Device.DeviceAddress.ToString()));

                // create new shell obj for database submission
                ShellTemp shellTemp = new ShellTemp
                {
                    Temperature = receivedData.Temperature,
                    RecordedDateTime = receivedData.RecordedDateTime,
                    Device = device
                };

                currentDevice.Temp.Add(shellTemp);
                currentDevice.DataPoints.Add((new DataPoint(DateTimeAxis.ToDouble(shellTemp.RecordedDateTime),
                    shellTemp.Temperature)));

                _shellRepo.Create(shellTemp); // create a new record in the database.
                _temperatureSubject.SetState(shellTemp);

                currentDevice.State.Message = BLTError.connected + currentDevice.DeviceName;
                SetConnectionStatus(currentDevice, DeviceConnectionStatus.CONNECTED);
            }
            catch (NullReferenceException ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogError(ex.Message);

                if (currentDevice != null && SelectedDevice == currentDevice)
                {
                    currentDevice.State.Message = BLTError.connecting + currentDevice.DeviceName;
                    SetConnectionStatus(currentDevice, DeviceConnectionStatus.CONNECTING);

                    ResetDeviceCounter(currentDevice); // maybe this needs to be outside this if???
                    ResetBluetoothClient(currentDevice);
                }
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogError(ex.Message);

                if (currentDevice != null && SelectedDevice == currentDevice)
                {
                    currentDevice.State.Message = ex.Message + " - " + currentDevice.DeviceName; 
                    SetConnectionStatus(currentDevice, DeviceConnectionStatus.FAILED);

                    ResetDeviceCounter(currentDevice);
                    ResetBluetoothClient(currentDevice);
                }
            }
            catch (SocketException ex)
            {
                Debug.WriteLine(ex.Message);
                _logger.LogError(ex.Message);

                if (currentDevice != null && SelectedDevice == currentDevice)
                {
                    StopCommand.Execute(null);

                    currentDevice.State.Message = BLTError.error + currentDevice.DeviceName;
                    SetConnectionStatus(currentDevice, DeviceConnectionStatus.FAILED);

                    ResetDeviceCounter(currentDevice);
                    ResetBluetoothClient(currentDevice);
                }
            }
            catch (IOException ex) // thermocouple is probably broken, unplugged or not working
            {
                _logger.LogError("Check the thermocouple, it might be broken");
                Debug.WriteLine("The thermocouple is suspected to not be working");
                Debug.WriteLine(ex.Message);

                if (currentDevice != null && SelectedDevice == currentDevice)
                {
                    //StopCommand.Execute(null);

                    currentDevice.State.Message = "The thermocouple is not working - " + currentDevice.DeviceName;
                    SetConnectionStatus(currentDevice, DeviceConnectionStatus.FAILED);

                    ResetDeviceCounter(currentDevice);
                    ResetBluetoothClient(currentDevice);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An exception occurred");
                Debug.WriteLine(ex.Message);
                _logger.LogError(ex.Message);
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
            if (!foundDevice.State.Equals(status) && SelectedDevice == foundDevice)
            {
                foundDevice.State.IsConnected = status;
                SetDeviceState(foundDevice.State);
            }
        }

        /// <summary>
        /// Update the connection status
        /// </summary>
        /// <param name="foundDevices">The foundDevices's connection status to update</param>
        private void SetDeviceState(ConnectionState foundDevice)
            => _subject.SetState(foundDevice);

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
