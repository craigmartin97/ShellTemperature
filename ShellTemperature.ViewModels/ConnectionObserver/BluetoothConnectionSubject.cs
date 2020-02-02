using ShellTemperature.Models;
using ShellTemperature.ViewModels.ViewModels.ConnectionStatus;
using System.Collections.Generic;

namespace ShellTemperature.ViewModels.ConnectionObserver
{
    public class BluetoothConnectionSubject
    {
        private readonly IList<BluetoothConnectionObserverViewModel> _observer = new List<BluetoothConnectionObserverViewModel>();
        private Device _device;

        public Device GetState() => _device;

        public void SetState(Device device)
        {
            _device = device;
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