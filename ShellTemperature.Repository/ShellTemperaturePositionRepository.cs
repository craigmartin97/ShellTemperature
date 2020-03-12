using System;
using System.Collections.Generic;
using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;

namespace ShellTemperature.Repository
{
    public class ShellTemperaturePositionRepository : IRepository<ShellTemperaturePosition>
    {
        public bool Create(ShellTemperaturePosition model)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ShellTemperaturePosition> GetAll()
        {
            throw new NotImplementedException();
        }

        public ShellTemperaturePosition GetItem(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Guid id)
        {
            throw new NotImplementedException();
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