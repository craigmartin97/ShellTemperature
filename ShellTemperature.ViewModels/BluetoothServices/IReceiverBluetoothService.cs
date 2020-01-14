namespace ShellTemperature.ViewModels.BluetoothServices
{
    public interface IReceiverBluetoothService
    {
        void Start();

        void Stop();

        string Data { get; set; }
    }
}
