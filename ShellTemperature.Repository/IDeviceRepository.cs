namespace ShellTemperature.Repository
{
    public interface IDeviceRepository<T> : IRepository<T>
    {
        T GetDevice(string deviceAddress);
    }
}
