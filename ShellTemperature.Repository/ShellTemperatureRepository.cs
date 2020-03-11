using Microsoft.EntityFrameworkCore;
using ShellTemperature.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using ShellTemperature.Repository.Interfaces;

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

            DeviceInfo dbDevice = _context.DevicesInfo.Find(model.Device.Id);
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
        => _context.ShellTemperatures.Include(dev => dev.Device);

        public bool Delete(Guid id)
        {
            ShellTemp shellTemp = _context.ShellTemperatures.Find(id);
            if (shellTemp == null)
                return false;

            _context.ShellTemperatures.Remove(shellTemp);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteRange(IEnumerable<ShellTemp> items)
        {
            if (items == null)
                return false;

            _context.ShellTemperatures.RemoveRange(items);
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Get all the shell temperature data between a date range.
        /// </summary>
        /// <param name="start">The start of the range to search for</param>
        /// <param name="end">The end of the range to search for</param>
        /// <returns>Returns an enumerable of shell temperatures</returns>
        public IEnumerable<ShellTemp> GetShellTemperatureData(DateTime start, DateTime end)
        {
            return _context.ShellTemperatures
                .Include(dev => dev.Device)
                .Where(dateTime => dateTime.RecordedDateTime >= start && dateTime.RecordedDateTime <= end);
        }

        /// <summary>
        /// Get the shell temperature data between two dates.
        /// Optional filter on the device name and device address can be provided
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="deviceName"></param>
        /// <param name="deviceAddress"></param>
        /// <returns></returns>
        public IEnumerable<ShellTemp> GetShellTemperatureData(DateTime start, DateTime end, string deviceName = null, string deviceAddress = null)
        {
            return _context.ShellTemperatures
                .Include(dev => dev.Device)
                .Where(device =>
                    string.IsNullOrWhiteSpace(deviceName) || device.Device.DeviceName.Equals(deviceName) &&
                    string.IsNullOrWhiteSpace(deviceAddress) || device.Device.DeviceAddress.Equals(deviceAddress) &&
                    device.RecordedDateTime >= start && device.RecordedDateTime <= end);
        }

        /// <summary>
        /// Get the item from the database matching the id
        /// </summary>
        /// <param name="id">Id of the item to retrieve</param>
        /// <returns>Returns the item from database matching the id value</returns>
        public ShellTemp GetItem(Guid id)
            => _context.ShellTemperatures.Find(id);
    }
}
