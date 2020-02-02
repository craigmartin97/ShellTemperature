using BluetoothService.BluetoothServices;
using BluetoothService.cs.BluetoothServices;
using OxyPlot;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;

namespace ShellTemperature.Models
{
    public class Device : INotifyPropertyChanged
    {
        public DispatcherTimer Timer { get; set; }

        public double CurrentData { get; set; }

        public ObservableCollection<ShellTemp> Temp { get; set; }

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

        /// <summary>
        /// Is the device reading data
        /// </summary>
        public bool IsRunning { get; set; }

        public IReceiverBluetoothService BluetoothService { get; set; }

        public BluetoothDevice BluetoothDevice { get; set; }

        public string DeviceName { get; set; }

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
