using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShellTemperature.Repository
{
    public class PositionsRepository : BaseRepository, IRepository<Positions>
    {
        public PositionsRepository(ShellDb context) : base(context) { }

        public bool Create(Positions model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The position model supplied is null");

            Positions dbDevicePosition = GetItem(model.Id);
            if (dbDevicePosition != null)
                return false;
            
            bool positionExists = Context.Positions.Any(x => x.Position.Equals(model.Position));
            if (positionExists)
                return false; // already exists

            Context.Positions.Add(model);
            Context.SaveChanges();
            return true;
        }

        public IEnumerable<Positions> GetAll()
            => Context.Positions;

        public Positions GetItem(Guid id)
            => Context.Positions.Find(id);

        public bool Delete(Guid id)
        {
            Positions dbDevicePosition = GetItem(id);
            if (dbDevicePosition == null)
                throw new NullReferenceException("Unable to find the position to delete");

            Context.Positions.Remove(dbDevicePosition);
            Context.SaveChanges();
            return true;
        }

        public bool DeleteRange(IEnumerable<Positions> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items), "The collection supplied was invalid as it was null");

            Context.Positions.RemoveRange(items);
            Context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Update the device position in the database
        /// </summary>
        /// <param name="model">The updated model object</param>
        /// <returns>Returns true if the interaction was sucecssful</returns>
        public bool Update(Positions model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The model supplied was null");

            // Find the item in the DB
            Positions dbDevicePosition = GetItem(model.Id);
            if (dbDevicePosition == null)
                throw new NullReferenceException("Could not find the item");

            // Search for any others with the same position name
            IEnumerable<Positions> allDevicePositions = GetAll();
            bool any = allDevicePositions.Where(devicePosition => devicePosition.Id != model.Id)
                .Select(devicePosition => devicePosition.Position.Equals(model.Position))
                .Any(x => x);

            if (any)
                return false; // Unable to update as conflicts with other!!!

            dbDevicePosition.Position = model.Position; // Update pos

            Context.SaveChanges();
            return true;
        }
    }
}