using BluetoothService.cs.BluetoothServices;

namespace BluetoothService.BluetoothServices
{
    public interface IReceiverBluetoothService
    {
        double? ReadData(BluetoothDevice device);

        void Stop();

        double GetBluetoothData();

        double? ConnectToDevice(BluetoothDevice device);
    }
}
