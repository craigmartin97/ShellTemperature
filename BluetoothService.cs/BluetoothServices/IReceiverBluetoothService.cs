using BluetoothService.Models;

namespace BluetoothService.BluetoothServices
{
    public interface IReceiverBluetoothService
    {
        DeviceReading ReadData(FoundBluetoothDevice device);

        void Stop();

        DeviceReading ConnectToDevice(FoundBluetoothDevice device);
    }
}
