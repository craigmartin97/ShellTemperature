using ShellTemperature.ViewModels.ConnectionObserver;

namespace ShellTemperature.ViewModels.ViewModels.ConnectionStatus
{
    public abstract class BluetoothConnectionObserverViewModel : ViewModelBase
    {
        protected BluetoothConnectionSubject _subject;
        public abstract void Update();
    }
}