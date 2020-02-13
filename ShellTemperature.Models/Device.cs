using System.Collections.Generic;
using BluetoothService.BluetoothServices;
using BluetoothService.cs.BluetoothServices;
using OxyPlot;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using BluetoothService.Enums;

namespace ShellTemperature.Models
{
    public class Device : INotifyPropertyChanged
    {
        public DispatcherTimer Timer { get; set; }

        public double CurrentData { get; set; }

        /// <summary>
        /// Stores all the shell readings, but excludes outliers
        /// </summary>
        public ObservableCollection<ShellTemp> Temp { get; set; }

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

        //private DeviceConnectionStatus _isConnected;
        ///// <summary>
        ///// Represents the status of the device connection status
        ///// </summary>
        //public DeviceConnectionStatus IsConnected
        //{
        //    get => _isConnected;
        //    set
        //    {
        //        _isConnected = value;
        //        OnPropertyChanged(nameof(IsConnected));
        //    }
        //}

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

        public int ReadingsCounter { get; set; }

        #region Constructors

        public Device()
        {

        }

        public Device(BluetoothDevice device, string deviceName)
        {
            Timer = new DispatcherTimer();
            CurrentData = 0;
            DataPoints = new ObservableCollection<DataPoint>();
            Temp = new ObservableCollection<ShellTemp>();
            BluetoothService = new ReceiverBluetoothService();
            BluetoothDevice = device;
            IsTimerEnabled = false;
            DeviceName = deviceName;
            AllTemperatureReadings = new List<double>();
            State = new ConnectionState();
        }
        #endregion 


        public event PropertyChangedEventHandler PropertyChanged;

        #region Notify Property Changed
        /// <summary>
        /// Inform the observers that the property has updated
        /// </summary>
        /// <param name="propertyName">The name of the property that has been updated</param>
        protected void OnPropertyChanged(string propertyName)
            => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
            => PropertyChanged?.Invoke(this, e);
        #endregion
    }
}
