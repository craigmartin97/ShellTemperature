using ShellTemperature.ViewModels.ConnectionObserver;
using System.Diagnostics;
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
            => Device = _subject?.GetState();

        #endregion
    }
}