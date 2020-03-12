using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
            DeviceInfo device = _context.DevicesInfo.Find(model.ShellTemp.Device.Id);
            ShellTemp temp = _context.ShellTemperatures.Find(model.ShellTemp.Id);
            ReadingComment readingComment = _context.ReadingComments.Find(model.Comment.Id);

            if (temp == null || device == null || readingComment == null)
                throw new NullReferenceException("The temperature, device or comment is null");

            // see if the temperature already has a comment
            ShellTemperatureComment exists = _context.ShellTemperatureComments.FirstOrDefault(x => x.ShellTemp.Id == temp.Id);
            if (exists != null) // already exists
            {
                exists.Comment = readingComment; // Update the record
            }
            else
            {
                model.ShellTemp = temp;
                model.ShellTemp.Device = device;
                model.Comment = readingComment;
                _context.ShellTemperatureComments.Add(model);
            }

            _context.SaveChanges();
            return true;
        }

        public IEnumerable<ShellTemperatureComment> GetAll()
            => _context.ShellTemperatureComments.Include(x => x.ShellTemp)
                .Include(x => x.Comment);

        public ShellTemperatureComment GetItem(Guid id)
            => _context.ShellTemperatureComments.Find(id);


        public bool Delete(Guid id)
        {
            ShellTemperatureComment comment = _context.ShellTemperatureComments.Find(id);
            if (comment == null)
                return false;

            _context.ShellTemperatureComments.Remove(comment);
            _context.SaveChanges();
            return true;
        }

        public bool DeleteRange(IEnumerable<ShellTemperatureComment> items)
        {
            throw new NotImplementedException();
        }

        public bool Update(ShellTemperatureComment model)
        {
            throw new NotImplementedException();
        }
    }
}