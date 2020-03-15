using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;

namespace ShellTemperature.Repository
{
    public class ShellTemperaturePositionRepository : IRepository<ShellTemperaturePosition>
    {
        private readonly ShellDb _context;

        public ShellTemperaturePositionRepository(ShellDb context)
        {
            _context = context;
        }

        public bool Create(ShellTemperaturePosition model)
        {
            // Validation
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The model supplied is null");
            if (model.ShellTemp?.Device == null || model.Position == null)
                throw new ArgumentNullException(nameof(model), "The model supplied is invalid as it has null references");

            ShellTemp dbShellTemp = _context.ShellTemperatures.Find(model.ShellTemp.Id);
            DeviceInfo dbDeviceInfo = _context.DevicesInfo.Find(model.ShellTemp.Device.Id);
            Positions dbDevicePosition = _context.Positions.Find(model.Position.Id);

            if (dbDeviceInfo != null && dbShellTemp != null)
            {
                model.ShellTemp = dbShellTemp;
                model.ShellTemp.Device = dbDeviceInfo;
                model.Position = dbDevicePosition;

                _context.ShellTemperaturePositions.Add(model);
                _context.SaveChanges();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get all of the shell temperature positions
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ShellTemperaturePosition> GetAll()
            => _context.ShellTemperaturePositions
                .Include(x => x.Position)
                .Include(x => x.ShellTemp);

        /// <summary>
        /// Find a single shell temp position record
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ShellTemperaturePosition GetItem(Guid id)
            => _context.ShellTemperaturePositions
                .Include(x => x.ShellTemp)
                .Include(x => x.ShellTemp)
                .FirstOrDefault(x => x.Id == id);


        public bool Delete(Guid id)
        {
            ShellTemperaturePosition dbShellTemperaturePosition = _context.ShellTemperaturePositions.Find(id);
            _context.ShellTemperaturePositions.Remove(dbShellTemperaturePosition);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteRange(IEnumerable<ShellTemperaturePosition> items)
        {
            throw new NotImplementedException();
        }

        public bool Update(ShellTemperaturePosition model)
        {
            throw new NotImplementedException();
        }
    }
}