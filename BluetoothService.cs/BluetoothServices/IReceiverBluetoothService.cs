using BluetoothService.cs.BluetoothServices;
using InTheHand.Net.Sockets;
using System.Collections.Generic;

namespace BluetoothService.BluetoothServices
{
    public interface IReceiverBluetoothService
    {
        void ReadData(BluetoothDevice device);

        void Stop();

        double GetBluetoothData();

        List<BluetoothDevice> GetBluetoothDevices();
    }
}
