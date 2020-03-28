using BluetoothService;
using BluetoothService.Enums;
using OxyPlot;
using ShellTemperature.Data;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace ShellTemperature.Models
{
    public class Device : ModelBase
    {
        public DispatcherTimer Timer { get; set; }

        private ObservableCollection<ShellTemperatureRecord> _temp;

        /// <summary>
        /// Stores all the shell readings, but excludes outliers
        /// </summary>
        public ObservableCollection<ShellTemperatureRecord> Temp
        {
            get => _temp;
            set
            {
                _temp = value;
                OnPropertyChanged(nameof(Temp));
            }
        }

        public ObservableCollection<DataPoint> DataPoints { get; set; }

        private bool _isTimerEnabled;

        public bool IsTimerEnabled
        {
            get => _isTimerEnabled;
            set
            {
                _isTimerEnabled = value;
                OnPropertyChanged(nameof(IsTimerEnabled));
            }
        }

        private ConnectionState _state;

        public ConnectionState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        private string _deviceName;

        public string DeviceName
        {
            get => _deviceName;
            set
            {
                _deviceName = value;
                OnPropertyChanged(nameof(DeviceName));
            }
        }

        public string DeviceAddress { get; set; }

        /// <summary>
        /// The current device position related to the
        /// the area the device is recording
        /// </summary>
        public Positions CurrentDevicePosition { get; set; }

        #region Constructors

        public Device()
        {

        }

        public Device(string deviceName, string deviceAddress)
        {
            Timer = new DispatcherTimer();
            DataPoints = new ObservableCollection<DataPoint>();
            Temp = new ObservableCollection<ShellTemperatureRecord>();
            IsTimerEnabled = false;
            DeviceName = deviceName;
            DeviceAddress = deviceAddress;
            State = new ConnectionState()
            {
                IsConnected = DeviceConnectionStatus.CONNECTING,
                Message = BLTError.connecting + deviceName
            };
        }
        #endregion
    }
}
