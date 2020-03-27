using BluetoothService.BluetoothServices;
using BluetoothService.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OxyPlot;
using ShellTemperature.Data;
using ShellTemperature.Models;
using ShellTemperature.Repository.Interfaces;
using ShellTemperature.ViewModels.ConnectionObserver;
using ShellTemperature.ViewModels.Outliers;
using ShellTemperature.ViewModels.TemperatureObserver;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ShellTemperature.ViewModels.Commands;
using OxyPlot.Axes;

namespace ShellTemperature.ViewModels.ViewModels.LadleShell
{
    public class LiveWifiAndBluetoothShellDataViewModel : BaseLiveShellDataViewModel
    {
        public override RelayCommand StartCommand
            => new RelayCommand(delegate
            {
                base.StartCommand.Execute(null);
                SelectedDevice.State.Message = "Connected To - " + SelectedDevice.DeviceName;
                SetConnectionStatus(SelectedDevice, DeviceConnectionStatus.CONNECTED);
            });

        #region Constructors
        public LiveWifiAndBluetoothShellDataViewModel(IBluetoothFinder bluetoothFinder,
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
            : base(bluetoothFinder, shellTemperatureRepository, sdCardShellTemperatureRepository, deviceRepository, configuration, subject,
                temperatureSubject, logger, outlierDetector, clear, commentRepository, readingCommentRepository,
                positionRepository, shellTempPositionRepository, sdCardCommentRepository)
        {
            IList<DeviceInfo> potentialWifiDevices = FindPotentialWifiDevices();

            foreach (DeviceInfo device in potentialWifiDevices.ToList())
            {
                DateTime start = DateTime.Now.AddMinutes(-2);
                DateTime end = DateTime.Now;

                WifiDevice wifiDevice = new WifiDevice(device.DeviceName, device.DeviceAddress, start);
                bool inUse = WifiDeviceInUse(wifiDevice, start, end, out ShellTemp[] recentTemps);
                if (!inUse)
                    potentialWifiDevices.Remove(device);
                else
                {
                    SetWifiDeviceDataReadings(wifiDevice, recentTemps);
                    SetWifiDeviceDataPoints(wifiDevice);
                    InstantiateNewDevice(wifiDevice);

                    wifiDevice.State.Message = "Connected To - " + wifiDevice.DeviceName;
                }
            }

            SetSelectedDeviceWhenNull();
            if (Devices.Count == 1) // only one in devices so, must be selected
            {
                SetConnectionStatus(SelectedDevice, DeviceConnectionStatus.CONNECTED);
            }

            SetCanRemoveDevices();
        }
        #endregion

        /// <summary>
        /// Check if the found wifi device is in use
        /// </summary>
        /// <param name="wifiDevices"></param>
        /// <param name="previousMinutes"></param>
        private bool WifiDeviceInUse(WifiDevice wifiDevice, DateTime start, DateTime end, out ShellTemp[] recentTemps)
        {
            recentTemps = GetDeviceData(start, end, wifiDevice);
            return recentTemps.Any();
        }

        #region Instansiate Device

        private void InstantiateNewDevice(WifiDevice wifiDevice)
        {
            wifiDevice.Timer.Tick += (sender, args) => Timer_Tick(wifiDevice);
            wifiDevice.Timer.Interval = new TimeSpan(0, 0, 30);
            wifiDevice.Timer.Start();

            Devices.Add(wifiDevice);
        }
        #endregion

        /// <summary>
        /// Find any potential Wifi devices
        /// </summary>
        /// <param name="foundDevices"></param>
        /// <returns></returns>
        private IList<DeviceInfo> FindPotentialWifiDevices()
            => _deviceRepository.GetAll().Where(dev => dev.DeviceType == DeviceType.Wifi).ToList();

        private void SetWifiDeviceDataReadings(Device device, IEnumerable<ShellTemp> temps)
        {
            device.Temp = new ObservableCollection<ShellTemperatureRecord>(temps
                .Select(temp => new ShellTemperatureRecord(temp.Id, temp.Temperature, temp.RecordedDateTime,
                    temp.Latitude, temp.Longitude, temp.Device))
                .OrderBy(x => x.RecordedDateTime));
        }

        private void SetWifiDeviceDataPoints(Device device)
        {
            foreach (var dataReading in device.Temp)
            {
                device.DataPoints.Add(new DataPoint(DateTimeAxis.ToDouble(dataReading.RecordedDateTime),
                    dataReading.Temperature));
            }
        }

        private ShellTemp[] GetDeviceData(DateTime start, DateTime end, WifiDevice device)
            => ShellTemperatureRepository.GetShellTemperatureData(start, end,
                device.DeviceName, device.DeviceAddress).ToArray();

        private void Timer_Tick(WifiDevice device)
        {
            // Last record datetime plus one second
            DateTime start = device.Temp[^1].RecordedDateTime.AddSeconds(1);
            DateTime end = DateTime.Now;

            bool inUse = WifiDeviceInUse(device, start, end, out ShellTemp[] recentTemps);

            if (!inUse)
            {
                if (device.FailureAttempts < 5)
                {
                    device.FailureAttempts++;
                    return;
                }

                // Has failed more than 5 times, remove device
                Application.Current.Dispatcher.Invoke(delegate
                {
                    Devices.Remove(device);
                    if (Devices.Count == 0)
                        SetConnectionStatus(); // Set to null and default message
                    else
                        SelectedDevice = Devices.FirstOrDefault(); // Select next item

                    device.Timer.Stop();
                });
            }
            else
            {
                // Latest Temperatures
                ShellTemperatureRecord[] temps = recentTemps.Select(temp
                    => new ShellTemperatureRecord(temp.Id, temp.Temperature, temp.RecordedDateTime,
                        temp.Latitude, temp.Longitude, temp.Device)).ToArray();

                foreach (ShellTemperatureRecord temp in temps)
                {
                    device.Temp.Add(temp);
                    device.DataPoints.Add(new DataPoint(DateTimeAxis.ToDouble(temp.RecordedDateTime), temp.Temperature));
                }

                SetConnectionStatus(device, DeviceConnectionStatus.CONNECTED);
            }
        }

        protected override FindDevices SearchForNearbyDevices()
        {
            FindDevices findDevices = base.SearchForNearbyDevices();

            IList<WifiDevice> wifiDevices = new List<WifiDevice>();

            IList<DeviceInfo> potentialWifiDevices = FindPotentialWifiDevices();

            foreach (DeviceInfo device in potentialWifiDevices.ToList())
            {
                // Already exists and in use so skip
                if (Devices.FirstOrDefault(dev => dev.DeviceAddress.Equals(device.DeviceAddress)) != null)
                    continue;

                DateTime start = DateTime.Now.AddMinutes(-2);
                DateTime end = DateTime.Now;

                WifiDevice wifiDevice = new WifiDevice(device.DeviceName, device.DeviceAddress, start);
                bool inUse = WifiDeviceInUse(wifiDevice, start, end, out ShellTemp[] recentTemps);
                if (inUse)
                {
                    SetWifiDeviceDataReadings(wifiDevice, recentTemps);
                    SetWifiDeviceDataPoints(wifiDevice);
                    wifiDevices.Add(wifiDevice);
                }
            }

            findDevices.WifiDevices = wifiDevices;
            return findDevices;
        }

        protected override void InstantiateSearchedForDevices(FindDevices findDevices)
        {
            base.InstantiateSearchedForDevices(findDevices);
            Application.Current.Dispatcher.Invoke(delegate
            {
                foreach (var wifiDevice in findDevices.WifiDevices)
                {
                    InstantiateNewDevice(wifiDevice);
                    wifiDevice.State.Message = "Connected To - " + wifiDevice.DeviceName;
                }

                SetSelectedDeviceWhenNull();
                if (Devices.Count == 1) // only one in devices so, must be selected
                {
                    SetConnectionStatus(SelectedDevice, DeviceConnectionStatus.CONNECTED);
                }
            });
        }
    }
}