using OxyPlot;
using ShellTemperature.Models;
using System.Collections.ObjectModel;

namespace ShellTemperature.ViewModels.ViewModels.LadleShell
{
    public abstract class BaseShellViewModel : ViewModelBase
    {
        #region Properties
        private ObservableCollection<ShellTemp> _bluetoothData = new ObservableCollection<ShellTemp>();
        /// <summary>
        /// A list collection of the live data being sent via bluetooth
        /// </summary>
        public ObservableCollection<ShellTemp> BluetoothData
        {
            get => _bluetoothData;
            protected set
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
            protected set
            {
                _dataPoints = value;
                OnPropertyChanged(nameof(DataPoints));
            }
        }

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
        public Device SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }
        #endregion
    }
}
