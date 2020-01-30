using BluetoothService.BluetoothServices;
using BluetoothService.cs.BluetoothServices;
using InTheHand.Net.Sockets;
using OxyPlot;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace ShellTemperature.Models
{
    public class Device
    {
        public DispatcherTimer Timer { get; set; }

        public double CurrentData { get; set; }

        public ObservableCollection<ShellTemp> Temp { get; set; } 

        public ObservableCollection<DataPoint> DataPoints { get; set; }

        public bool IsTimerEnabled { get; set; }

        public IReceiverBluetoothService BluetoothService { get; set; }

        public BluetoothDevice BluetoothDevice { get; set; }
    }
}
