using System;
using System.Collections.Generic;
using System.Linq;
using ShellTemperature.Models;

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
            DeviceInfo alreadyExists = _context.Devices.FirstOrDefault(x => x.DeviceAddress.Equals(model.DeviceAddress));

            if (alreadyExists != null)
                throw new ArgumentException("The device " + model.DeviceAddress + " already exists in the data store");

            _context.Add(model);
            _context.SaveChanges();
            return true;
        }

        public IEnumerable<DeviceInfo> GetAll()
            => _context.Devices;

        /// <summary>
        /// Find the device by the device address.
        /// </summary>
        /// <param name="deviceAddress">The device address</param>
        /// <returns>Returns a device object if found, else returns null</returns>
        public DeviceInfo GetDevice(string deviceAddress)
            => _context.Devices.FirstOrDefault(x => x.DeviceAddress.Equals(deviceAddress));

    }
}