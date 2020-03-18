using Microsoft.EntityFrameworkCore;
using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShellTemperature.Repository
{
    public class SdCardShellTemperatureCommentRepository : BaseRepository, IRepository<SdCardShellTemperatureComment>
    {
        public SdCardShellTemperatureCommentRepository(ShellDb context) : base(context) { }

        public bool Create(SdCardShellTemperatureComment model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The comment object was null");

            // get data from database
            DeviceInfo device = Context.DevicesInfo.Find(model.SdCardShellTemp.Device.Id);
            SdCardShellTemp temp = Context.SdCardShellTemperatures.Find(model.SdCardShellTemp.Id);
            ReadingComment readingComment = Context.ReadingComments.Find(model.Comment.Id);

            if (temp == null || device == null || readingComment == null)
                throw new NullReferenceException("The temperature, device or comment is null");

            // see if the temperature already has a comment
            SdCardShellTemperatureComment exists = Context.SdCardShellTemperatureComments.FirstOrDefault(x => x.SdCardShellTemp.Id == temp.Id);
            if (exists != null) // already exists
            {
                exists.Comment = readingComment; // Update the record
            }
            else
            {
                model.SdCardShellTemp = temp;
                model.SdCardShellTemp.Device = device;
                model.Comment = readingComment;
                Context.SdCardShellTemperatureComments.Add(model);
            }

            Context.SaveChanges();
            return true;
        }

        public IEnumerable<SdCardShellTemperatureComment> GetAll()
            => Context.SdCardShellTemperatureComments.Include(x => x.SdCardShellTemp)
                .Include(x => x.Comment);

        public SdCardShellTemperatureComment GetItem(Guid id)
            => Context.SdCardShellTemperatureComments.Find(id);

        public bool Delete(Guid id)
        {
            SdCardShellTemperatureComment comment = Context.SdCardShellTemperatureComments.Find(id);
            if (comment == null)
                throw new NullReferenceException("Could not find the comment to delete");

            Context.SdCardShellTemperatureComments.Remove(comment);
            Context.SaveChanges();
            return true;
        }

        public bool DeleteRange(IEnumerable<SdCardShellTemperatureComment> items)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items), "The collection supplied was invalid");

            Context.SdCardShellTemperatureComments.RemoveRange(items);
            Context.SaveChanges();
            return true;
        }

        public bool Update(SdCardShellTemperatureComment model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The shell temperature comment supplied is null");

            SdCardShellTemperatureComment shellTemperatureComment = GetItem(model.Id);
            if (shellTemperatureComment == null)
                throw new NullReferenceException("Could not find the shell temperature");

            shellTemperatureComment.Comment = model.Comment;
            shellTemperatureComment.SdCardShellTemp = model.SdCardShellTemp;
            Context.SaveChanges();
            return true;
        }
    }
}