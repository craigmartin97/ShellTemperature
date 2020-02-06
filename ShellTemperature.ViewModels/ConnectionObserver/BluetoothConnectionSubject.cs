using ShellTemperature.Models;
using ShellTemperature.ViewModels.ViewModels.ConnectionStatus;
using System.Collections.Generic;

namespace ShellTemperature.ViewModels.ConnectionObserver
{
    public class BluetoothConnectionSubject
    {
        private readonly IList<BluetoothConnectionObserverViewModel> _observer = new List<BluetoothConnectionObserverViewModel>();
        private Device _foundDevices;

        public Device GetState() => _foundDevices;

        public void SetState(Device foundDevices)
        {
            _foundDevices = foundDevices;
            NotifyAllObservers();
        }

        public void Attach(BluetoothConnectionObserverViewModel observer)
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