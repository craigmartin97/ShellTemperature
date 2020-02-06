using BluetoothService.cs.BluetoothServices;
using BluetoothService.Models;

namespace BluetoothService.BluetoothServices
{
    public interface IReceiverBluetoothService
    {
        DeviceReading ReadData(BluetoothDevice device);

        void Stop();

        DeviceReading ConnectToDevice(BluetoothDevice device);
    }
}
