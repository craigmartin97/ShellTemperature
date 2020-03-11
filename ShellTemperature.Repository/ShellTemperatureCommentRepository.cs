using System;
using System.Collections.Generic;
using ShellTemperature.Models;

namespace ShellTemperature.Repository
{
    public class ShellTemperatureCommentRepository : IRepository<ShellTemperatureComment>
    {
        private readonly ShellDb _context;

        public ShellTemperatureCommentRepository(ShellDb context)
        {
            _context = context;
        }

        public bool Create(ShellTemperatureComment model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The comment object was null");

            // get data from database
            DeviceInfo device = _context.Devices.Find(model.ShellTemp.Device.Id);
            ShellTemp temp = _context.ShellTemperatures.Find(model.ShellTemp.Id);

            if(temp == null || device == null)
                throw new NullReferenceException("The temperature or device is null");

            model.ShellTemp = temp;
            model.ShellTemp.Device = device;

            _context.ShellTemperatureComments.Add(model);
            _context.SaveChanges();
            return true;
        }

        public IEnumerable<ShellTemperatureComment> GetAll()
        {
            throw new NotImplementedException();
        }

        public ShellTemperatureComment GetItem(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool DeleteRange(IEnumerable<ShellTemperatureComment> items)
        {
            throw new NotImplementedException();
        }
    }
}