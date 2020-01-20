namespace ShellTemperature.ViewModels.BluetoothServices
{
    public interface IReceiverBluetoothService
    {
        void ReadData();

        void Stop();

        double GetBluetoothData();
    }
}
