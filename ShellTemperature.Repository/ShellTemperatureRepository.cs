using ShellTemperature.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShellTemperature.Repository
{
    public class ShellTemperatureRepository : IShellTemperatureRepository<ShellTemp> //IRepository<Models.ShellTemp>
    {
        private readonly ShellDb _context;

        public ShellTemperatureRepository(ShellDb context)
        {
            _context = context;
        }

        public bool Create(ShellTemp model)
        {
            if (model != null)
            {
                _context.Add(model);
                _context.SaveChangesAsync();
                return true;
            }

            return false;
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
