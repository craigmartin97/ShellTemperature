using System;
using System.Collections.Generic;

namespace ShellTemperature.Repository.Interfaces
{
    public interface IShellTemperatureRepository<T> : IRepository<T>
    {
        IEnumerable<T> GetShellTemperatureData(DateTime start, DateTime end, string deviceName = null, string deviceAddress = null);
    }
}
