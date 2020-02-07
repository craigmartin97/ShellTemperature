using ShellTemperature.ViewModels.Interfaces;
using ShellTemperature.ViewModels.TemperatureObserver;

namespace ShellTemperature.ViewModels.ViewModels.TemperatureNotifier
{
    public abstract class TemperatureNotifierViewModel : ViewModelBase, IUpdate
    {
        protected TemperatureSubject _temperatureSubject;
        public abstract void Update();
    }
}