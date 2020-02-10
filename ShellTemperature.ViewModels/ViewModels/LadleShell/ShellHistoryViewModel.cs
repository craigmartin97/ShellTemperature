using OxyPlot;
using OxyPlot.Axes;
using ShellTemperature.Models;
using ShellTemperature.Repository;
using ShellTemperature.ViewModels.TemperatureObserver;
using ShellTemperature.ViewModels.ViewModels.TemperatureNotifier;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using ShellTemperature.ViewModels.Commands;

namespace ShellTemperature.ViewModels.ViewModels.LadleShell
{
    /// <summary>
    /// ViewModel for retrieving and displaying previous shell history data to the users
    /// </summary>
    public class ShellHistoryViewModel : TemperatureNotifierViewModel
    {
        #region Fields
        private readonly IShellTemperatureRepository<ShellTemp> _shellTempRepo;

        /// <summary>
        /// Device repository to get information about past used devices
        /// </summary>
        private readonly IDeviceRepository<DeviceInfo> _deviceRepository;

        /// <summary>
        /// The temperature subject to get and notify of state changes
        /// </summary>
        private readonly TemperatureSubject _temperatureSubject;
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


        private ObservableCollection<DeviceInfo> _devices;
        /// <summary>
        /// Collection of all devices used previously to record data
        /// </summary>
        public ObservableCollection<DeviceInfo> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                OnPropertyChanged(nameof(Devices));
            }
        }

        private DeviceInfo _currentDeviceInfo;
        /// <summary>
        /// The current selected device to live the temperature data for
        /// </summary>
        public DeviceInfo CurrentDeviceInfo
        {
            get => _currentDeviceInfo;
            set
            {
                _currentDeviceInfo = value;
                OnPropertyChanged(nameof(CurrentDeviceInfo));
            }
        }

        private ObservableCollection<ShellTemp> _selectedCells;

        public ObservableCollection<ShellTemp> SelectedCells
        {
            get => _selectedCells;
            set
            {
                _selectedCells = value;
                OnPropertyChanged(nameof(SelectedCells));
            }
        }

        public RelayCommand PressMe =>
            new RelayCommand(param =>
            {
                var dt = param as DataGrid;
            });
        #endregion

        #region Commands

        #endregion

        #region Constructors
        public ShellHistoryViewModel(IShellTemperatureRepository<ShellTemp> shellTemperature,
            IDeviceRepository<DeviceInfo> deviceRepository, TemperatureSubject subject)
        {
            _shellTempRepo = shellTemperature;
            _deviceRepository = deviceRepository;
            _temperatureSubject = subject;

            _temperatureSubject.Attach(this);

            // get the last seven days of temps and display to the user in the list.
            UpdateDevices();
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

        /// <summary>
        /// Update the devices collection
        /// </summary>
        private void UpdateDevices()
        {
            Devices = new ObservableCollection<DeviceInfo>(_deviceRepository.GetAll());
            if (Devices.Count > 0)
                CurrentDeviceInfo = Devices[0];
        }
        #endregion

        /// <summary>
        /// Update the latest temperatures to include the latest readings
        /// from the current device
        /// </summary>
        public override void Update()
        {
            ShellTemp latestReading = _temperatureSubject?.GetState();

            if (latestReading == null) return;

            if (latestReading.Device.DeviceAddress.Equals(CurrentDeviceInfo.DeviceAddress)
            && End.Date.Equals(DateTime.Today))
            {
                BluetoothData.Add(latestReading);
                DataPoints.Add(new DataPoint(DateTimeAxis.ToDouble(latestReading.RecordedDateTime),
                    latestReading.Temperature));
            }
        }
    }
}
