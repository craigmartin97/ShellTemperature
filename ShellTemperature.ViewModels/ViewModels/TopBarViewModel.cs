using ShellTemperature.Models;
using ShellTemperature.ViewModels.ConnectionObserver;
using ShellTemperature.ViewModels.ViewModels.ConnectionStatus;

namespace ShellTemperature.ViewModels.ViewModels
{
    public class TopBarViewModel : BluetoothConnectionObserverViewModel
    {
        #region Properties

        private ConnectionState _device;

        /// <summary>
        /// The current foundDevices
        /// </summary>
        public ConnectionState Device
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
            if (subject == null) return;

            _subject = subject;
            _subject.Attach(this);
        }
        #endregion

        #region Updator
        /// <summary>
        /// 
        /// </summary>
        public override void Update()
        {
            Device = _subject?.GetState();

            if (Device == null) return;

            ConnectionMessage = Device.Message;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public override void Update(string message)
        {
            Device = _subject?.GetState();

            if (Device == null) return;

            ConnectionMessage = message;
        }
        #endregion
    }
}