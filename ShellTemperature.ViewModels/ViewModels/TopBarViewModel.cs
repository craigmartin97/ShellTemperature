using ShellTemperature.ViewModels.ConnectionObserver;
using System.Diagnostics;
using BluetoothService.Enums;
using ShellTemperature.Models;
using ShellTemperature.ViewModels.ViewModels.ConnectionStatus;

namespace ShellTemperature.ViewModels.ViewModels
{
    public class TopBarViewModel : BluetoothConnectionObserverViewModel
    {
        #region Properties

        private Device _device;

        /// <summary>
        /// The current device
        /// </summary>
        public Device Device
        {
            get => _device;
            set
            {
                _device = value;
                OnPropertyChanged(nameof(Device));
            }
        }

        private string _connectionMessage;

        public string ConnectionMessage
        {
            get => _connectionMessage;
            set
            {
                _connectionMessage = value;
                OnPropertyChanged(nameof(ConnectionMessage));
            }
        }
        #endregion

        #region Constructors
        public TopBarViewModel(BluetoothConnectionSubject subject)
        {
            _subject = subject;
            _subject.Attach(this);
        }
        #endregion

        #region Updator

        public override void Update()
        {
            Device = _subject?.GetState();
            ConnectionMessage = GetConnectionStatus();
        }

        #endregion

        private string GetConnectionStatus()
        {
            return Device.IsConnected switch
            {
                BluetoothService.Enums.DeviceConnectionStatus.CONNECTED => ("Connected - " + Device.DeviceName),
                BluetoothService.Enums.DeviceConnectionStatus.CONNECTING => ("Connecting to Device - " + Device.DeviceName),
                BluetoothService.Enums.DeviceConnectionStatus.FAILED => ("Failed to Connect - " + Device.DeviceName),
                _ => "Error"
            };
        }
    }
}