using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShellTemperature.Service
{
    public interface IShellTemperatureService<T> :IService<T>
    {
        Task<IEnumerable<T>> GetShellTemperatureData(DateTime start, DateTime end);

        Task<IEnumerable<T>> GetShellTemperatureData(DateTime start, DateTime end, string deviceName = null, string deviceAddress = null);
    }
}