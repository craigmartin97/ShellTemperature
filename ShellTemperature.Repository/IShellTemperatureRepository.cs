﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ShellTemperature.Repository
{
    public interface IShellTemperatureRepository<T> : IRepository<T>
    {
        IEnumerable<T> GetShellTemperatureData(DateTime start, DateTime end);
    }
}