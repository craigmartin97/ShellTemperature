using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShellTemperature.Repository
{
    public class DevicesRepository : IDeviceRepository<DeviceInfo>
    {
        #region Fields

        private ShellDb _context;
        #endregion

        #region Constructors
        public DevicesRepository(ShellDb context)
        {
            _context = context;
        }
        #endregion

        public bool Create(DeviceInfo model)
        {
            DeviceInfo alreadyExists = _context.DevicesInfo.FirstOrDefault(x => x.DeviceAddress.Equals(model.DeviceAddress));

            if (alreadyExists != null)
                throw new ArgumentException("The device " + model.DeviceAddress + " already exists in the data store");

            _context.Add(model);
            _context.SaveChanges();
            return true;
        }

        public IEnumerable<DeviceInfo> GetAll()
            => _context.DevicesInfo;

        public bool Delete(Guid id)
        {
            DeviceInfo dev = _context.DevicesInfo.FirstOrDefault(x => x.Id.Equals(id));
            if (dev == null)
                return false;

            _context.DevicesInfo.Remove(dev);
            return true;
        }

        public bool DeleteRange(IEnumerable<DeviceInfo> items)
        {
            if (items == null)
                return false;

            _context.DevicesInfo.RemoveRange(items);
            _context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Find the device by the device address.
        /// </summary>
        /// <param name="deviceAddress">The device address</param>
        /// <returns>Returns a device object if found, else returns null</returns>
        public DeviceInfo GetDevice(string deviceAddress)
            => _context.DevicesInfo.FirstOrDefault(x => x.DeviceAddress.Equals(deviceAddress));

        /// <summary>
        /// Get a single device from the database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DeviceInfo GetItem(Guid id)
            => _context.DevicesInfo.FirstOrDefault(x => x.Id.Equals(id));
    }
}