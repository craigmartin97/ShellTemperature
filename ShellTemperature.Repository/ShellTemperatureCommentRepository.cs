using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ShellTemperature.Repository
{
    public class ShellTemperatureCommentRepository : BaseRepository, IRepository<ShellTemperatureComment>
    {
        public ShellTemperatureCommentRepository(ShellDb context) : base(context) { }

        public bool Create(ShellTemperatureComment model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The comment object was null");

            // get data from database
            DeviceInfo device = Context.DevicesInfo.Find(model.ShellTemp.Device.Id);
            ShellTemp temp = Context.ShellTemperatures.Find(model.ShellTemp.Id);
            ReadingComment readingComment = Context.ReadingComments.Find(model.Comment.Id);

            if (temp == null || device == null || readingComment == null)
                throw new NullReferenceException("The temperature, device or comment is null");

            // see if the temperature already has a comment
            ShellTemperatureComment exists = Context.ShellTemperatureComments.FirstOrDefault(x => x.ShellTemp.Id == temp.Id);
            if (exists != null) // already exists
            {
                exists.Comment = readingComment; // Update the record
            }
            else
            {
                model.ShellTemp = temp;
                model.ShellTemp.Device = device;
                model.Comment = readingComment;
                Context.ShellTemperatureComments.Add(model);
            }

            Context.SaveChanges();
            return true;
        }

        public IEnumerable<ShellTemperatureComment> GetAll()
            => Context.ShellTemperatureComments.Include(x => x.ShellTemp)
                .Include(x => x.Comment);

        public ShellTemperatureComment GetItem(Guid id)
            => Context.ShellTemperatureComments.Find(id);

        public bool Delete(Guid id)
        {
            ShellTemperatureComment comment = Context.ShellTemperatureComments.Find(id);
            if (comment == null)
                throw new NullReferenceException("Could not find the comment to delete");

            Context.ShellTemperatureComments.Remove(comment);
            Context.SaveChanges();
            return true;
        }

        public bool DeleteRange(IEnumerable<ShellTemperatureComment> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items), "The collection supplied was invalid");

            Context.ShellTemperatureComments.RemoveRange(items);
            Context.SaveChanges();
            return true;
        }

        public bool Update(ShellTemperatureComment model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The shell temperature comment supplied is null");

            ShellTemperatureComment shellTemperatureComment = GetItem(model.Id);
            if (shellTemperatureComment == null)
                throw new NullReferenceException("Could not find the shell temperature");

            shellTemperatureComment.Comment = model.Comment;
            shellTemperatureComment.ShellTemp = model.ShellTemp;
            Context.SaveChanges();
            return true;
        }
    }
}