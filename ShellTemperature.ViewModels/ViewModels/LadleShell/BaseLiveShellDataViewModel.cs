﻿using BluetoothService;
using BluetoothService.BluetoothServices;
using BluetoothService.Enums;
using BluetoothService.Models;
using CustomDialog.Dialogs;
using CustomDialog.Enums;
using CustomDialog.Services;
using InTheHand.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OxyPlot;
using OxyPlot.Axes;
using ShellTemperature.Data;
using ShellTemperature.Models;
using ShellTemperature.Repository.Interfaces;
using ShellTemperature.Service;
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ShellTemperature.ViewModels.ViewModels.LadleShell
{
    public abstract class BaseLiveShellDataViewModel : BaseLadleShellDataViewModel
    {
        #region Private fields
        /// <summary>
        /// The device repository to interact with the data store
        /// </summary>
        protected internal readonly IDeviceRepository<DeviceInfo> _deviceRepository;

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
        private readonly OutlierDetector _outlierDetector;
        private readonly ClearList _clear;

        /// <summary>
        /// Logger to record system messages
        /// </summary>
        private readonly ILogger<LiveBluetoothOnlyShellDataViewModel> _logger;

        /// <summary>
        /// Position repository is responsible for storing positions
        /// that a device is recording from on the ladle
        /// </summary>
        private readonly IRepository<Positions> _positionRepository;

        /// <summary>
        /// Shell temperature repository to store the position of each shell temperature
        /// </summary>
        private readonly IRepository<ShellTemperaturePosition> _shellTempPositionRepository;
        #endregion

        #region Properties
        private ObservableCollection<Device> _devices = new ObservableCollection<Device>();
        /// <summary>
        /// A collection of devices that bluetooth data can be retrieved from
        /// </summary>
        public ObservableCollection<Device> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
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
                _selectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
                if (value == null)
                {
                    return;
                }

                _subject.SetState(value.State);
            }
        }

        private ObservableCollection<Positions> _positions;
        /// <summary>
        /// Collection of positions that the selected device is reading from.
        /// i.e. the top of the ladle, side of the ladle, bottom of the ladle
        /// </summary>
        public ObservableCollection<Positions> Positions
        {
            get => _positions;
            set
            {
                _positions = value;
                OnPropertyChanged(nameof(Positions));
            }
        }

        private Positions _selectedPosition;
        /// <summary>
        /// The selected position is the position that has been chosen by
        /// the user to identify what position the current SelectedDevice is
        /// reading from
        /// </summary>
        public Positions SelectedPosition
        {
            get => _selectedPosition;
            set
            {
                _selectedPosition = value;
                OnPropertyChanged(nameof(SelectedPosition));
            }
        }

        private string _newPosition;
        /// <summary>
        /// New position that has been entered by the user
        /// in the editable text box
        /// </summary>
        public string NewPosition
        {
            get => _newPosition;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;
                if (SelectedPosition != null && SelectedPosition.Position.Equals(value))
                {
                    _newPosition = SelectedPosition.Position;
                    OnPropertyChanged(nameof(NewPosition));
                    return; // this item was selected
                }
                if (Positions.Any(x => x.Position.Equals(value))) // this text already exists in the coll
                {
                    Positions pos = Positions.FirstOrDefault(x => x.Position.Equals(value));
                    if (pos != null)
                        SelectedPosition = pos;

                    if (SelectedPosition != null)
                        _newPosition = SelectedPosition.Position;

                    OnPropertyChanged(nameof(NewPosition));
                    return;
                }

                _newPosition = value;
                SelectedPosition = new Positions(value);
                OnPropertyChanged(nameof(NewPosition));
            }
        }

        private string _latestLatitude = "N/A";
        /// <summary>
        /// Latest latitude value that has been recorded for the selected sensor
        /// </summary>
        public string LatestLatitude
        {
            get => _latestLatitude;
            set
            {
                _latestLatitude = value;
                OnPropertyChanged(nameof(LatestLatitude));
            }
        }

        private string _latestLongitude = "N/A";
        /// <summary>
        /// Latest longitude value that has been recorded for the selected sensor
        /// </summary>
        public string LatestLongitude
        {
            get => _latestLongitude;
            set
            {
                _latestLongitude = value;
                OnPropertyChanged(nameof(LatestLongitude));
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

        private bool _canRemoveDevice;
        /// <summary>
        /// Property indicating if devices can be removed from
        /// </summary>
        public bool CanRemoveDevice
        {
            get => _canRemoveDevice;
            set
            {
                _canRemoveDevice = value;
                OnPropertyChanged(nameof(CanRemoveDevice));
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Start reading the bluetooth service
        /// </summary>
        public virtual RelayCommand StartCommand
        => new RelayCommand(delegate
        {
            if (SelectedDevice == null)
                return;

            SelectedDevice.Timer.Start();
            SelectedDevice.IsTimerEnabled = false; // disable the foundDevices start button as already running.
        });

        /// <summary>
        /// Stop command stops the timer from running and stops the execution
        /// of reading data from the bluetooth foundDevices.
        /// </summary>
        public RelayCommand StopCommand
        => new RelayCommand(delegate
        {
            if (SelectedDevice == null)
                return;

            SelectedDevice.Timer.Stop(); // stop the current selected timer.
            SelectedDevice.IsTimerEnabled = true; // enable the start button for the timer

            // can't change connection status if the foundDevices has failed to connect.
            if (SelectedDevice.State.IsConnected.Equals(DeviceConnectionStatus.FAILED)) return;

            SelectedDevice.State.Message = BLTError.paused + SelectedDevice.DeviceName;
            SetConnectionStatus(SelectedDevice, DeviceConnectionStatus.PAUSED);
        });

        /// <summary>
        /// Search for any new bluetooth devices that do not currently exist
        /// </summary>
        public RelayCommand SearchForDevices
        => new RelayCommand(delegate
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    // disable the search functionality
                    Application.Current.Dispatcher?.Invoke(() => IsSearchForDevicesEnabled = false);

                    FindDevices findDevices = SearchForNearbyDevices();

                    if (findDevices.BluetoothDevices.Count == 0 && findDevices.WifiDevices.Count == 0)
                        return;

                    DialogResult result = DialogResult.Undefined;
                    // ask user if they want to start recording data from new devices
                    Application.Current.Dispatcher?.Invoke(() =>
                    {
                        DialogService service = new DialogService();
                        ConfirmationDialogViewModel confirmation = new ConfirmationDialogViewModel
                        ("Found New Devices",
                            "Found " + (findDevices.BluetoothDevices.Count + findDevices.WifiDevices.Count) + " device(s). Would you like to start recording data?");
                        result = service.OpenDialogService(confirmation);
                    });

                    // the user did not press yes.
                    if (result == DialogResult.Undefined || result == DialogResult.No)
                        return;

                    InstantiateSearchedForDevices(findDevices);

                    // set the datastore devices as new onces could have been added.
                    _datastoreDevices = _deviceRepository.GetAll();
                }
                finally
                {
                    OrderDevices();
                    Application.Current.Dispatcher?.Invoke(() => IsSearchForDevicesEnabled = true);
                    SetSelectedDeviceWhenNull();
                    SetCanRemoveDevices();
                }
            });

            thread.Start();
        });

        /// <summary>
        /// Search for nearby devices.
        /// </summary>
        /// <returns>Returns a collection of find devices made up of bluetooth and wifi devices</returns>
        protected virtual FindDevices SearchForNearbyDevices()
        {
            // Get nearby bluetooth devices
            IList<FoundBluetoothDevice> allDevicesFound = _bluetoothFinder.GetBluetoothDevices();

            // Filter out existing devices
            for (int i = 0; i < allDevicesFound.Count; i++) // each foundDevices found
            {
                for (int j = 0; j < Devices.Count; j++) // each foundDevices already found
                {
                    if (Devices[j].DeviceAddress
                        .Equals(allDevicesFound[i].Device.DeviceAddress)) // the foundDevices already exists
                    {
                        allDevicesFound.Remove(allDevicesFound[i]);
                    }
                }
            }

            return new FindDevices
            {
                BluetoothDevices = allDevicesFound,
                WifiDevices = new List<WifiDevice>() // just create a blank list. Children will override
            };
        }

        /// <summary>
        /// Instantiate new devices, set of there timers to start logging data regular
        /// </summary>
        /// <param name="findDevices"></param>
        protected virtual void InstantiateSearchedForDevices(FindDevices findDevices)
        {
            // user pressed yes so add devices
            foreach (FoundBluetoothDevice device in findDevices.BluetoothDevices)
            {
                BluetoothDevice dev = new BluetoothDevice(device, GetValueFromDeviceNameDictionary(device), device.Device.DeviceAddress.ToString());

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
        }

        /// <summary>
        /// Command to remove the selected device from the display
        /// </summary>
        public RelayCommand RemoveSelectedDevice =>
        new RelayCommand(delegate
        {
            if (SelectedDevice == null || Devices.Count == 0) return;

            SelectedDevice.Timer.Stop();
            if (SelectedDevice is BluetoothDevice dev)
                dev.FoundBluetoothDevice.Client.Close();

            Devices.Remove(SelectedDevice);

            if (Devices == null || Devices.Count == 0) // nothing in the collection
            {
                SelectedDevice = null;
                SetConnectionStatus();
            }
            else SelectedDevice = Devices.FirstOrDefault();

            SetCanRemoveDevices();
        });

        /// <summary>
        /// Submit the position for the selected device
        /// </summary>
        public RelayCommand SubmitPosition
        => new RelayCommand(delegate
        {
            if (SelectedPosition == null || SelectedDevice == null) // nothing has been entered or selected
                return;

            if (string.IsNullOrWhiteSpace(SelectedPosition.Position))
                return;

            if (SelectedPosition.Id.Equals(default)) // the Guid is a new entry
                _positionRepository.Create(SelectedPosition);

            bool foundNull = false;
            for (int i = SelectedDevice.Temp.Count - 1; i >= 0; i--)
            {
                ShellTemperatureRecord record = SelectedDevice.Temp[i];
                // Has a position
                if (!string.IsNullOrWhiteSpace(record.Position))
                {
                    // If already found a null, then we must have found them all so stop.
                    if (foundNull)
                        break;

                    continue;
                }

                foundNull = true;
                // Doesn't have a position
                record.Position = SelectedPosition.Position;
                ShellTemp shellTemp = new ShellTemp(record.Id, record.Temperature, record.RecordedDateTime,
                    record.Latitude, record.Longitude, record.Device);

                ShellTemperaturePosition position = new ShellTemperaturePosition
                {
                    ShellTemp = shellTemp,
                    Position = SelectedPosition
                };

                // Create a new shell temp position for the record
                _shellTempPositionRepository.Create(position);
            }

            // Set the selected devices current position to the selected position
            SelectedDevice.CurrentDevicePosition = SelectedPosition;
        });
        #endregion

        #region Constructors
        public BaseLiveShellDataViewModel(IBluetoothFinder bluetoothFinder,
            IShellTemperatureRepository<ShellTemp> shellTemperatureRepository,
            IShellTemperatureRepository<SdCardShellTemp> sdCardShellTemperatureRepository,
            IDeviceRepository<DeviceInfo> deviceRepository,
            IConfiguration configuration,
            BluetoothConnectionSubject subject,
            TemperatureSubject temperatureSubject,
            ILogger<LiveBluetoothOnlyShellDataViewModel> logger,
            OutlierDetector outlierDetector,
            ClearList clear,
            IRepository<ShellTemperatureComment> commentRepository,
            IReadingCommentRepository<ReadingComment> readingCommentRepository,
            IRepository<Positions> positionRepository,
            IRepository<ShellTemperaturePosition> shellTempPositionRepository,
            IRepository<SdCardShellTemperatureComment> sdCardCommentRepository)
            : base(readingCommentRepository, commentRepository, shellTemperatureRepository,
                sdCardShellTemperatureRepository, sdCardCommentRepository)
        {
            _bluetoothFinder = bluetoothFinder;
            _deviceRepository = deviceRepository;
            _subject = subject;
            _temperatureSubject = temperatureSubject;
            _logger = logger;
            _outlierDetector = outlierDetector;
            _clear = clear;
            _positionRepository = positionRepository;
            _shellTempPositionRepository = shellTempPositionRepository;

            // Assign positions
            Positions = new ObservableCollection<Positions>(_positionRepository.GetAll());

            //get the devices section from the config settings
            IEnumerable<IConfigurationSection> configDevices = configuration
                .GetSection("Devices").GetChildren();

            // covert the devices to a dictionary
            _deviceDictionary = configDevices.ToDictionary(
               dev => dev.Key, dev => dev.Value);

            List<FoundBluetoothDevice> bluetoothDevices = bluetoothFinder.GetBluetoothDevices();

            foreach (FoundBluetoothDevice device in bluetoothDevices)
            {
                BluetoothDevice dev = new BluetoothDevice(device, GetValueFromDeviceNameDictionary(device), device.Device.DeviceAddress.ToString());

                // set to connecting, in the constructor its being setup so its a connecting status
                SetConnectionStatus(dev, DeviceConnectionStatus.CONNECTING);

                dev.Timer.Tick += (sender, args) => Timer_Tick(dev);
                dev.Timer.Interval = new TimeSpan(0, 0, 1);
                dev.Timer.Start();

                // check if device exists in the data store
                CheckIfDeviceExistsAndCreate(dev);

                Devices.Add(dev);
            }

            if (Devices.Count == 0)
            {
                SetConnectionStatus();
                return;
            }

            OrderDevices();
            SetSelectedDeviceWhenNull();

            SetCanRemoveDevices();

            // get all the devices from the data store, this is needed
            _datastoreDevices = _deviceRepository.GetAll();
        }
        #endregion

        #region Timer Ticker

        /// <summary>
        /// Timer_tick executes the start command and then retrieves the bluetooth data.
        /// </summary>
        private void Timer_Tick(BluetoothDevice currentDevice)
        {
            try
            {
                // made 5000 recordings, to prevent overflow exception clear the lists
                if (_clear.Clear && currentDevice.AllTemperatureReadings.Count % _clear.ClearValue == 0)
                {
                    Process process = Process.GetCurrentProcess();
                    _logger.LogDebug("Used - " + process.PrivateMemorySize64);
                    _logger.LogDebug("Recorded X recordings clearing the data to prevent overflow");

                    currentDevice.AllTemperatureReadings.Clear();
                    currentDevice.DataPoints.Clear();
                    currentDevice.Temp.Clear();
                }

                DeviceReading receivedData = currentDevice.BluetoothService.ReadData(currentDevice.FoundBluetoothDevice);

                if (receivedData?.LiveDeviceReading == null)
                    throw new NullReferenceException("The sensor returned a null response");

                if (!receivedData.LiveDeviceReading.Temperature.HasValue)
                    throw new NullReferenceException("The temperature was invalid");

                // is the date and time recorded valid?
                if (receivedData.LiveDeviceReading.RecordedDateTime == null ||
                    !IsDateTimeValid(receivedData.LiveDeviceReading.RecordedDateTime))
                {
                    throw new InvalidOperationException("Invalid Date & Time, Try and Reset " +
                                                        "the DateTime Module - " + currentDevice.DeviceName);
                }

                double temperature = (double)receivedData.LiveDeviceReading.Temperature;
                currentDevice.AllTemperatureReadings.Add(temperature);

                // check if outlier
                bool isOutlier =
                    _outlierDetector.IsOutlier(currentDevice.AllTemperatureReadings, temperature);

                if (isOutlier) // value is outlier cannot be added
                    return;

                // get the device from the datastore collection
                DeviceInfo device = _datastoreDevices.FirstOrDefault(x =>
                    x.DeviceAddress.Equals(currentDevice.FoundBluetoothDevice.Device.DeviceAddress.ToString()));

                // create new shell obj for database submission
                ShellTemp shellTemp = new ShellTemp(temperature, (DateTime)receivedData.LiveDeviceReading.RecordedDateTime,
                    receivedData.LiveDeviceReading.Latitude, receivedData.LiveDeviceReading.Longitude, device);

                // Has SD card data
                if (receivedData.SdCardDeviceReading?.Temperature != null)
                {
                    SdCardShellTemp sdShellTemp = new SdCardShellTemp((double)receivedData.SdCardDeviceReading.Temperature,
                        receivedData.SdCardDeviceReading.RecordedDateTime,
                        receivedData.SdCardDeviceReading.Latitude, receivedData.SdCardDeviceReading.Longitude, device);
                    SdCardShellTemperatureRepository.Create(sdShellTemp);
                }

                if (SelectedDevice == currentDevice)
                {
                    LatestLatitude = receivedData.LiveDeviceReading.Latitude == null ? "N/A" : receivedData.LiveDeviceReading.Latitude.ToString();
                    LatestLongitude = receivedData.LiveDeviceReading.Longitude == null ? "N/A" : receivedData.LiveDeviceReading.Longitude.ToString();
                }

                ShellTemperatureRecord record = new ShellTemperatureRecord
                (shellTemp.Id, shellTemp.Temperature, shellTemp.RecordedDateTime, shellTemp.Latitude, shellTemp.Longitude,
                 shellTemp.Device);

                currentDevice.Temp.Add(record);
                IEnumerable<ShellTemperatureRecord> shellTemps = currentDevice.Temp.OrderBy(x => x.RecordedDateTime);
                currentDevice.Temp = new ObservableCollection<ShellTemperatureRecord>(shellTemps);

                currentDevice.DataPoints.Add((new DataPoint(DateTimeAxis.ToDouble(shellTemp.RecordedDateTime),
                    shellTemp.Temperature)));

                ShellTemperatureRepository.Create(shellTemp); // create a new record in the database.

                // Create a new shell temperature position
                StoreShellTempPosition(currentDevice, shellTemp, record);

                _temperatureSubject.SetState(record);
                record.Id = shellTemp.Id;

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

                    ResetBluetoothClient(currentDevice);
                }
            }
            catch (IOException ex) // thermocouple is probably broken, unplugged or not working
            {
                _logger.LogError("Check the thermocouple, it might be broken");
                Debug.WriteLine("The thermocouple is suspected to not be working");
                Debug.WriteLine(ex.Message);

                if (SelectedDevice.State.IsConnected == DeviceConnectionStatus.SLEEP)
                {
                    return; // don't do anything
                }

                if (currentDevice != null && SelectedDevice == currentDevice)
                {
                    currentDevice.State.Message = "The thermocouple is not working - " + currentDevice.DeviceName;
                    SetConnectionStatus(currentDevice, DeviceConnectionStatus.FAILED);

                    ResetBluetoothClient(currentDevice);
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                Debug.WriteLine("The index was out of range whilst extracting the data from the bluetooth device");
                Debug.WriteLine(ex.Message);
            }
            catch (SleepException ex)
            {
                if (currentDevice != null && SelectedDevice == currentDevice)
                {
                    currentDevice.State.Message = "The Device Is Sleeping - " + currentDevice.DeviceName;
                    SetConnectionStatus(currentDevice, DeviceConnectionStatus.SLEEP);
                }
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Close the bluetooth connection.
        /// Reset the connection object to be able to reconnect later.
        /// </summary>
        /// <param name="foundDevices">The devices connection to reset</param>
        private void ResetBluetoothClient(BluetoothDevice foundDevices)
        {
            // reset foundDevices
            foundDevices.FoundBluetoothDevice.Client.Close();
            foundDevices.FoundBluetoothDevice.Client = new BluetoothClient();
        }

        /// <summary>
        /// Set connection status to none
        /// </summary>
        protected void SetConnectionStatus()
        {
            ConnectionState state = new ConnectionState
            {
                IsConnected = DeviceConnectionStatus.NONE,
                Message = "No Devices Found"
            };

            SetDeviceState(state);
        }

        /// <summary>
        /// Change the connection status for the foundDevices
        /// </summary>
        /// <param name="foundDevices">The devices connection status to change</param>
        /// <param name="status">The status to check and change to</param>
        protected void SetConnectionStatus(Device foundDevice, DeviceConnectionStatus status)
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
        private string GetValueFromDeviceNameDictionary(FoundBluetoothDevice device)
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
        private void CheckIfDeviceExistsAndCreate(BluetoothDevice device)
        {
            // check if the device exists
            string deviceAddress = device.FoundBluetoothDevice.Device.DeviceAddress.ToString();
            DeviceInfo dataStoreDevice = _deviceRepository.GetDevice(deviceAddress);

            // the device doesn't exist
            if (dataStoreDevice == null)
            {
                _deviceRepository.Create(new DeviceInfo
                {
                    DeviceName = device.DeviceName,
                    DeviceAddress = deviceAddress,
                    DeviceType = DeviceType.Bluetooth // Must be blt as searched for nearby blt devices
                });
            }
        }

        /// <summary>
        /// Validate that the date and time is between two ranges
        /// </summary>
        /// <param name="currentDateTime">The date and time to validate</param>
        /// <returns>Returns true if the datetime is valid</returns>
        private bool IsDateTimeValid(DateTime? currentDateTime)
        {
            // problem with setting time with DS3231 module as gets time at compile 
            DateTime minDateTimeRange = DateTime.Now.AddMinutes(-5);
            DateTime maxDateTimeRange = DateTime.Now.AddMinutes(5);

            // current datetime is only valid if its between the two ranges
            return minDateTimeRange < currentDateTime && maxDateTimeRange > currentDateTime;
        }

        /// <summary>
        /// Order the devices in alphabetical order
        /// </summary>
        private void OrderDevices()
        {
            // order the devices
            IEnumerable<Device> orderedDevices = Devices.OrderBy(x => x.DeviceName);
            Devices = new ObservableCollection<Device>(orderedDevices);
        }

        /// <summary>
        /// Function to set the property can remove devices
        /// based upon the number of devices inside the collection.
        /// If the collection has items then they can be removed.
        /// Otherwise, they cannot.
        /// </summary>
        protected void SetCanRemoveDevices()
         => CanRemoveDevice = Devices.Count > 0;

        /// <summary>
        /// Set the selected device when it is null
        /// with the first item from the devices list.
        /// </summary>
        protected void SetSelectedDeviceWhenNull()
        {
            if (SelectedDevice == null && Devices.Count > 0)
                SelectedDevice = Devices[0];
        }

        private void StoreShellTempPosition(Device currentDevice, ShellTemp shellTemp, ShellTemperatureRecord record)
        {
            if (currentDevice.CurrentDevicePosition != null)
            {
                record.Position = currentDevice.CurrentDevicePosition.Position;
                ShellTemperaturePosition shellTemperaturePosition = new ShellTemperaturePosition
                {
                    ShellTemp = shellTemp,
                    Position = currentDevice.CurrentDevicePosition
                };
                _shellTempPositionRepository.Create(shellTemperaturePosition);
            }
        }
        #endregion
    }
}