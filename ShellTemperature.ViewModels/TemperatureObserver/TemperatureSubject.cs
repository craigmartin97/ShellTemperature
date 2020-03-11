using ShellTemperature.Data;
using ShellTemperature.ViewModels.ViewModels.TemperatureNotifier;
using System.Collections.Generic;

namespace ShellTemperature.ViewModels.TemperatureObserver
{
    public class TemperatureSubject : Interfaces.ISubject<TemperatureNotifierViewModel>
    {
        private readonly IList<TemperatureNotifierViewModel> _observer = new List<TemperatureNotifierViewModel>();
        private ShellTemp _shellTemp;

        public ShellTemp GetState()
            => _shellTemp;

        public void SetState(ShellTemp shellTemp)
        {
            _shellTemp = shellTemp;
            NotifyAllObservers();
        }

        public void Attach(TemperatureNotifierViewModel observer)
            => _observer.Add(observer);

        public void NotifyAllObservers()
        {
            foreach (var observer in _observer)
            {
                observer.Update();
            }
        }
    }
}
