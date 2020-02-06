using ShellTemperature.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShellTemperature.Repository
{
    public class ShellTemperatureRepository : IShellTemperatureRepository<ShellTemp>
    {
        private readonly ShellDb _context;

        public ShellTemperatureRepository(ShellDb context)
        {
            _context = context;
        }

        public bool Create(ShellTemp model)
        {
            if (model?.Device == null) return false;

            DeviceInfo dbDevice = _context.Devices.Find(model.Device.Id);
            DeviceInfo device = dbDevice ?? model.Device;
            model.Device = device;

            _context.Add(model);
            _context.SaveChanges();
            return true;



        }

        /// <summary>
        /// Get all the shell temperature data
        /// </summary>
        /// <returns>Returns an enumerable collection of all the temperature data</returns>
        public IEnumerable<ShellTemp> GetAll()
        => _context.ShellTemperatures;

        /// <summary>
        /// Get all the shell temperature data between a date range.
        /// </summary>
        /// <param name="start">The start of the range to search for</param>
        /// <param name="end">The end of the range to search for</param>
        /// <returns>Returns an enumerable of shell temperatures</returns>
        public IEnumerable<ShellTemp> GetShellTemperatureData(DateTime start, DateTime end)
        => _context.ShellTemperatures.Where(x => x.RecordedDateTime >= start && x.RecordedDateTime <= end);

    }
}
