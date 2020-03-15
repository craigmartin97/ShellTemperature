using BluetoothService;
using BluetoothService.BluetoothServices;
using BluetoothService.cs.BluetoothServices;
using BluetoothService.Enums;
using OxyPlot;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using ShellTemperature.Data;

namespace ShellTemperature.Models
{
    public class Device : ModelBase
    {
        public DispatcherTimer Timer { get; set; }

        public double CurrentData { get; set; }

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

        /// <summary>
        /// Stores all the temperature points regardless if they are outliers or not.
        /// </summary>
        public List<double> AllTemperatureReadings { get; set; }

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

        public IReceiverBluetoothService BluetoothService { get; set; }

        public BluetoothDevice BluetoothDevice { get; set; }

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

        /// <summary>
        /// The current device position related to the
        /// the area the device is recording
        /// </summary>
        public Positions CurrentDevicePosition { get; set; }

        #region Constructors

        public Device()
        {

        }

        public Device(BluetoothDevice device, string deviceName)
        {
            Timer = new DispatcherTimer();
            CurrentData = 0;
            DataPoints = new ObservableCollection<DataPoint>();
            Temp = new ObservableCollection<ShellTemperatureRecord>();
            BluetoothService = new ReceiverBluetoothService();
            BluetoothDevice = device;
            IsTimerEnabled = false;
            DeviceName = deviceName;
            AllTemperatureReadings = new List<double>();
            State = new ConnectionState()
            {
                IsConnected = DeviceConnectionStatus.CONNECTING,
                Message = BLTError.connecting + deviceName
            };
        }
        #endregion
    }
}
