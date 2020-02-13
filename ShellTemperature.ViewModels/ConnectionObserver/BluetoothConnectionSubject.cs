using ShellTemperature.Models;
using ShellTemperature.ViewModels.ViewModels.ConnectionStatus;
using System.Collections.Generic;

namespace ShellTemperature.ViewModels.ConnectionObserver
{
    public class BluetoothConnectionSubject
    {
        private readonly IList<BluetoothConnectionObserverViewModel> _observer = new List<BluetoothConnectionObserverViewModel>();
        private ConnectionState _connStatus;

        public ConnectionState GetState() => _connStatus;

        public void SetState(ConnectionState connStatus)
        {
            _connStatus = connStatus;
            NotifyAllObservers();
        }

        public void SetState(ConnectionState connStatus, string message)
        {
            _connStatus = connStatus;
            NotifyAllObservers(message);
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

        public void NotifyAllObservers(string message)
        {
            foreach (var observer in _observer)
            {
                observer.Update(message);
            }
        }
    }
}