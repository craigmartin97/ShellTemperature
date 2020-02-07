using ShellTemperature.ViewModels.ConnectionObserver;
using ShellTemperature.ViewModels.Interfaces;

namespace ShellTemperature.ViewModels.ViewModels.ConnectionStatus
{
    public abstract class BluetoothConnectionObserverViewModel : ViewModelBase, IUpdate
    {
        protected BluetoothConnectionSubject _subject;
        public abstract void Update();
    }
}