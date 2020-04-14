using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;

namespace ShellTemperature.Repository
{
    public class ShellTemperaturePositionRepository : BaseRepository, IRepository<ShellTemperaturePosition>
    {
        public ShellTemperaturePositionRepository(ShellDb context) : base(context) { }

        public bool Create(ShellTemperaturePosition model)
        {
            // Validation
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The model supplied is null");
            if (model.ShellTemp?.Device == null || model.Position == null)
                throw new ArgumentNullException(nameof(model), "The model supplied is invalid as it has null references");

            ShellTemp dbShellTemp = Context.ShellTemperatures.Find(model.ShellTemp.Id);
            DeviceInfo dbDeviceInfo = Context.DevicesInfo.Find(model.ShellTemp.Device.Id);
            Positions dbDevicePosition = Context.Positions.Find(model.Position.Id);

            if (dbDeviceInfo != null && dbShellTemp != null)
            {
                model.ShellTemp = dbShellTemp;
                model.ShellTemp.Device = dbDeviceInfo;
                model.Position = dbDevicePosition;

                Context.ShellTemperaturePositions.Add(model);
                Context.SaveChanges();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get all of the shell temperature positions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ShellTemperaturePosition> GetAll()
            => Context.ShellTemperaturePositions
                .Include(x => x.Position)
                .Include(x => x.ShellTemp);

        /// <summary>
        /// Find a single shell temp position record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ShellTemperaturePosition GetItem(Guid id)
            => Context.ShellTemperaturePositions
                .Include(x => x.Position)
                .Include(x => x.ShellTemp)
                .FirstOrDefault(x => x.Id == id);


        public bool Delete(Guid id)
        {
            ShellTemperaturePosition dbShellTemperaturePosition = Context.ShellTemperaturePositions.Find(id);
            if (dbShellTemperaturePosition == null)
                throw new NullReferenceException("Could not find the shell temperature position in the database");

            Context.ShellTemperaturePositions.Remove(dbShellTemperaturePosition);
            Context.SaveChanges();
            return true;
        }

        public bool DeleteRange(IEnumerable<ShellTemperaturePosition> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items), "The collection supplied was null");

            Context.ShellTemperaturePositions.RemoveRange(items);
            Context.SaveChanges();
            return true;
        }

        public bool Update(ShellTemperaturePosition model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The moded supplied was invalid");

            ShellTemperaturePosition dbShellTemperaturePosition = GetItem(model.Id);
            if (dbShellTemperaturePosition == null)
                throw new NullReferenceException("Could not find the shell temperature position in the database");

            dbShellTemperaturePosition.Position = model.Position;
            dbShellTemperaturePosition.ShellTemp = model.ShellTemp;
            Context.SaveChanges();
            return true;
        }
    }
}