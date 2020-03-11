namespace ShellTemperature.Repository.Interfaces
{
    public interface IDeviceRepository<T> : IRepository<T>
    {
        T GetDevice(string deviceAddress);
    }
}
