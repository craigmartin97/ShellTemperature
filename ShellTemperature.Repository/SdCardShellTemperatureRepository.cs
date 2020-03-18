using Microsoft.EntityFrameworkCore;
using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShellTemperature.Repository
{
    public class SdCardShellTemperatureRepository : BaseRepository, IShellTemperatureRepository<SdCardShellTemp>
    {
        public SdCardShellTemperatureRepository(ShellDb context) : base(context) { }

        public bool Create(SdCardShellTemp model)
        {
            if (model?.Device == null)
                throw new ArgumentNullException(nameof(model), "The model supplied was invalid");

            DeviceInfo dbDevice = Context.DevicesInfo.Find(model.Device.Id);
            DeviceInfo device = dbDevice ?? model.Device;
            model.Device = device;

            Context.Add(model);
            Context.SaveChanges();
            return true;
        }

        public IEnumerable<SdCardShellTemp> GetAll()
            => Context.SdCardShellTemperatures.Include(x => x.Device);

        public bool Delete(Guid id)
        {
            SdCardShellTemp shellTemp = Context.SdCardShellTemperatures.Find(id);
            if (shellTemp == null)
                return false;

            Context.SdCardShellTemperatures.Remove(shellTemp);
            Context.SaveChanges();
            return true;
        }

        public bool DeleteRange(IEnumerable<SdCardShellTemp> items)
        {
            if (items == null)
                return false;

            foreach (var shellTemp in items)
            {
                SdCardShellTemp dbTemp = Context.SdCardShellTemperatures.Find(shellTemp.Id);
                Context.SdCardShellTemperatures.Remove(dbTemp);
            }

            Context.SaveChanges();
            return true;
        }

        public bool Update(SdCardShellTemp model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The shell temperature was invalid");

            SdCardShellTemp dbShellTemp = GetItem(model.Id);
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
        public IEnumerable<SdCardShellTemp> GetShellTemperatureData(DateTime start, DateTime end)
        {
            return Context.SdCardShellTemperatures
                .Include(dev => dev.Device)
                .Where(dateTime => (dateTime != null) && dateTime.RecordedDateTime >= start && dateTime.RecordedDateTime <= end);
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
        public IEnumerable<SdCardShellTemp> GetShellTemperatureData(DateTime start, DateTime end, string deviceName = null, string deviceAddress = null)
        {
            return Context.SdCardShellTemperatures
                .Include(dev => dev.Device)
                .Where(device =>
                    string.IsNullOrWhiteSpace(deviceName) || device.Device.DeviceName.Equals(deviceName) &&
                    string.IsNullOrWhiteSpace(deviceAddress) || device.Device.DeviceAddress.Equals(deviceAddress) &&
                    (device.RecordedDateTime != null) &&
                    device.RecordedDateTime >= start && device.RecordedDateTime <= end);
        }

        /// <summary>
        /// Get the item from the database matching the id
        /// </summary>
        /// <param name="id">Id of the item to retrieve</param>
        /// <returns>Returns the item from database matching the id value</returns>
        public SdCardShellTemp GetItem(Guid id)
            => Context.SdCardShellTemperatures.Find(id);
    }
}