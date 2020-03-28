﻿using Microsoft.EntityFrameworkCore;
using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShellTemperature.Repository
{
    public class ShellTemperatureRepository : BaseRepository, IShellTemperatureRepository<ShellTemp>
    {
        public ShellTemperatureRepository(ShellDb context) : base(context) { }

        public async Task<bool> Create(ShellTemp model)
        {
            if (model?.Device == null)
                throw new ArgumentNullException(nameof(model), "The model supplied was invalid");

            // Try and find the device in the database
            DeviceInfo dbDevice = await Context.DevicesInfo.FindAsync(model.Device.Id) ??
                                  await Context.DevicesInfo.FirstOrDefaultAsync(dev =>
                                      dev.DeviceAddress.Equals(model.Device.DeviceAddress));

            DeviceInfo device = dbDevice ?? model.Device; // Use the database device or add models device
            model.Device = device;

            await Context.AddAsync(model);
            await Context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Get all the shell temperature data
        /// </summary>
        /// <returns>Returns an enumerable collection of all the temperature data</returns>
        public IEnumerable<ShellTemp> GetAll()
        => Context.ShellTemperatures.Include(dev => dev.Device);

        public bool Delete(Guid id)
        {
            ShellTemp shellTemp = Context.ShellTemperatures.Find(id);
            if (shellTemp == null)
                return false;

            Context.ShellTemperatures.Remove(shellTemp);
            Context.SaveChanges();
            return true;
        }

        public bool DeleteRange(IEnumerable<ShellTemp> items)
        {
            if (items == null)
                return false;

            foreach (var shellTemp in items)
            {
                ShellTemp dbTemp = Context.ShellTemperatures.Find(shellTemp.Id);
                Context.ShellTemperatures.Remove(dbTemp);
            }

            Context.SaveChanges();
            return true;
        }

        public bool Update(ShellTemp model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The shell temperature was invalid");

            ShellTemp dbShellTemp = GetItem(model.Id);
            if (dbShellTemp == null)
                throw new NullReferenceException("Could not find the shell temperature in the database");

            DeviceInfo dbDeviceInfo = Context.DevicesInfo.Find(model.Device.Id);
            if (dbDeviceInfo == null)
                throw new NullReferenceException("Could not find the device in the database");

            dbShellTemp.Temperature = model.Temperature;
            dbShellTemp.RecordedDateTime = model.RecordedDateTime;
            dbShellTemp.Latitude = model.Latitude;
            dbShellTemp.Longitude = model.Longitude;
            dbShellTemp.Device = dbDeviceInfo;

            Context.SaveChanges();
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
            return Context.ShellTemperatures
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
            return Context.ShellTemperatures
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
            => Context.ShellTemperatures.Find(id);
    }
}
