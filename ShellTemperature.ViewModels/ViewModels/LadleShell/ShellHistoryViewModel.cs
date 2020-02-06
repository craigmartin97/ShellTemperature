using OxyPlot;
using OxyPlot.Axes;
using ShellTemperature.Models;
using ShellTemperature.Repository;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;

namespace ShellTemperature.ViewModels.ViewModels.LadleShell
{
    /// <summary>
    /// ViewModel for retrieving and displaying previous shell history data to the users
    /// </summary>
    public class ShellHistoryViewModel : BaseShellViewModel
    {
        #region Fields
        private readonly IShellTemperatureRepository<ShellTemp> _shellTempRepo;

        /// <summary>
        /// Device repository to get information about past used devices
        /// </summary>
        private readonly IDeviceRepository<DeviceInfo> _deviceRepository;

        /// <summary>
        /// Collection of all devices used previously to record data
        /// </summary>
        private readonly IEnumerable<DeviceInfo> _devices;
        #endregion

        #region Properties
        private ObservableCollection<ShellTemp> _bluetoothData = new ObservableCollection<ShellTemp>();
        /// <summary>
        /// A list collection of the live data being sent via bluetooth
        /// </summary>
        public ObservableCollection<ShellTemp> BluetoothData
        {
            get => _bluetoothData;
            set
            {
                _bluetoothData = value;
                OnPropertyChanged(nameof(BluetoothData));
            }
        }

        private ObservableCollection<DataPoint> _dataPoints = new ObservableCollection<DataPoint>();
        /// <summary>
        /// The data points on the oxyplot graph
        /// </summary>
        public ObservableCollection<DataPoint> DataPoints
        {
            get => _dataPoints;
            set
            {
                _dataPoints = value;
                OnPropertyChanged(nameof(DataPoints));
            }
        }

        /// <summary>
        /// The private start date field. By default, (OnLoad), get the last
        /// seven days of shell temperatures
        /// </summary>
        private DateTime _start = DateTime.Now.AddDays(-7);
        /// <summary>
        /// The start date to start searching for data shell records from
        /// </summary>
        public DateTime Start
        {
            get => _start;
            set
            {
                if (value > End) // the Start date cannot be after the end
                    return;

                _start = value;
                OnPropertyChanged(nameof(Start));

                SetBluetoothData();
                SetDataPoints();
            }
        }

        /// <summary>
        /// The private end date field. By default set to the current date time 
        /// to search between
        /// </summary>
        private DateTime _end = DateTime.Now;
        /// <summary>
        /// The end date to stop search for to get ladle shell temperature records for
        /// </summary>
        public DateTime End
        {
            get => _end;
            set
            {
                if (value < Start) // the end date cannot be before the start date
                    return;

                _end = value;
                OnPropertyChanged(nameof(End));

                SetBluetoothData();
                SetDataPoints();
            }
        }

        private DeviceInfo _currentDeviceInfo;

        public DeviceInfo CurrentDeviceInfo
        {
            get => _currentDeviceInfo;
            set
            {
                _currentDeviceInfo = value;
                OnPropertyChanged(nameof(CurrentDeviceInfo));
            }
        }
        #endregion

        #region Commands

        #endregion

        #region Constructors
        public ShellHistoryViewModel(IShellTemperatureRepository<ShellTemp> shellTemperature,
            IDeviceRepository<DeviceInfo> deviceRepository)
        {
            _shellTempRepo = shellTemperature;
            _deviceRepository = deviceRepository;

            _devices = _deviceRepository.GetAll();
            CurrentDeviceInfo = _devices.FirstOrDefault();

            // get the last seven days of temps and display to the user in the list.
            SetBluetoothData();
            SetDataPoints();
        }
        #endregion

        #region Set Data
        /// <summary>
        /// Sets the bluetooth data collection.
        /// Retrieves the shell temperatures between the start date and end date.
        /// </summary>
        private void SetBluetoothData()
        {
            if (CurrentDeviceInfo == null) return;

            BluetoothData = new ObservableCollection<ShellTemp>(_shellTempRepo.GetShellTemperatureData(Start, End,
                CurrentDeviceInfo.DeviceName, CurrentDeviceInfo.DeviceAddress));
        }

        /// <summary>
        /// Go through all of the bluetooth data and set all the points on the graph accordingly
        /// </summary>
        private void SetDataPoints()
        {
            // have points to plot?
            if (BluetoothData.Count == 0) return;

            DataPoints = new ObservableCollection<DataPoint>(); // reset collection
            foreach (ShellTemp temp in BluetoothData) // plot points
            {
                DataPoints.Add(new DataPoint(DateTimeAxis.ToDouble(temp.RecordedDateTime), temp.Temperature));
            }
        }
        #endregion
    }
}
