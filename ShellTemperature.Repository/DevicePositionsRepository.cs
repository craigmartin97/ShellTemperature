using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using System;
using System.Collections.Generic;

namespace ShellTemperature.Repository
{
    public class DevicePositionsRepository : IRepository<DevicePosition>
    {
        private readonly ShellDb _context;
        public DevicePositionsRepository(ShellDb context)
        {
            _context = context;
        }

        public bool Create(DevicePosition model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The position model supplied is null");

            _context.Positions.Add(model);
            _context.SaveChanges();
            return true;
        }

        public IEnumerable<DevicePosition> GetAll()
            => _context.Positions;

        public DevicePosition GetItem(Guid id)
            => _context.Positions.Find(id);

        public bool Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool DeleteRange(IEnumerable<DevicePosition> items)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Update the device position in the database
        /// </summary>
        /// <param name="model">The updated model object</param>
        /// <returns>Returns true if the interaction was sucecssful</returns>
        public bool Update(DevicePosition model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The model supplied was null");

            // Find the item in the DB
            DevicePosition dbDevicePosition = GetItem(model.Id);
            if (dbDevicePosition == null)
                throw new NullReferenceException("Could not find the item");

            dbDevicePosition.Position = model.Position; // Update pos

            _context.SaveChanges();
            return true;
        }
    }
}