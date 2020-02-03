using BluetoothService.cs.BluetoothServices;

namespace BluetoothService.BluetoothServices
{
    public interface IReceiverBluetoothService
    {
        double? ReadData(BluetoothDevice device);

        void Stop();

        double? ConnectToDevice(BluetoothDevice device);
    }
}
