using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ShellTemperature.Repository
{
    public class DevicesRepository : BaseRepository, IDeviceRepository<DeviceInfo>
    {
        #region Constructors
        public DevicesRepository(ShellDb context) : base(context) { }
        #endregion

        /// <summary>
        /// Create a new device item in the database
        /// </summary>
        /// <param name="model">The device to create</param>
        /// <returns></returns>
        public async Task<bool> Create(DeviceInfo model)
        {
            DeviceInfo alreadyExists = await Context.DevicesInfo.FirstOrDefaultAsync(x => x.DeviceAddress.Equals(model.DeviceAddress));

            if (alreadyExists != null)
                throw new ArgumentException("The device " + model.DeviceAddress + " already exists in the data store");

            await Context.AddAsync(model);
            await Context.SaveChangesAsync();
            return true;
        }

        public IEnumerable<DeviceInfo> GetAll()
            => Context.DevicesInfo;

        public bool Delete(Guid id)
        {
            DeviceInfo dev = Context.DevicesInfo.FirstOrDefault(x => x.Id.Equals(id));
            if (dev == null)
                return false;

            ShellTemp shellTemp = Context.ShellTemperatures.FirstOrDefault(x => x.Device.Id == id);
            if (shellTemp != null)
                return false; // can't delete as this device is linked to a temp

            Context.DevicesInfo.Remove(dev);
            return true;
        }

        /// <summary>
        /// Delete a collection of devices
        /// </summary>
        /// <param name="items">The devices to remove</param>
        /// <returns></returns>
        public bool DeleteRange(IEnumerable<DeviceInfo> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items), "The collection supplied was null");

            Context.DevicesInfo.RemoveRange(items);
            Context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Update an existing record
        /// </summary>
        /// <param name="model">The device object to update</param>
        /// <returns></returns>
        public bool Update(DeviceInfo model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The model supplied is null");

            DeviceInfo dbDeviceInfo = GetItem(model.Id);
            if (dbDeviceInfo == null)
                throw new NullReferenceException("Could not find the device to update");

            IEnumerable<DeviceInfo> allDeviceInfos = GetAll();
            bool alreadyExists = allDeviceInfos.Where(device => device.Id != model.Id)
                .Select(device => device.DeviceAddress.Equals(model.DeviceAddress)
                || device.DeviceName.Equals(model.DeviceName))
                .Any(x => x);

            if (alreadyExists)
                return false;

            // Update each attribute
            dbDeviceInfo.DeviceAddress = model.DeviceAddress;
            dbDeviceInfo.DeviceName = model.DeviceName;

            Context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Find the device by the device address.
        /// </summary>
        /// <param name="deviceAddress">The device address</param>
        /// <returns>Returns a device object if found, else returns null</returns>
        public DeviceInfo GetDevice(string deviceAddress)
            => Context.DevicesInfo.FirstOrDefault(x => x.DeviceAddress.Equals(deviceAddress));

        /// <summary>
        /// Get a single device from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DeviceInfo GetItem(Guid id)
            => Context.DevicesInfo.FirstOrDefault(x => x.Id.Equals(id));
    }
}