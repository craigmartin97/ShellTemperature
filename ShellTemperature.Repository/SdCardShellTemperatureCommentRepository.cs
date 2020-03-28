using Microsoft.EntityFrameworkCore;
using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShellTemperature.Repository
{
    public class SdCardShellTemperatureCommentRepository : BaseRepository, IRepository<SdCardShellTemperatureComment>
    {
        public SdCardShellTemperatureCommentRepository(ShellDb context) : base(context) { }

        public async Task<bool> Create(SdCardShellTemperatureComment model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The comment object was null");

            // get data from database
            DeviceInfo device = await Context.DevicesInfo.FindAsync(model.SdCardShellTemp.Device.Id);
            SdCardShellTemp temp = await Context.SdCardShellTemperatures.FindAsync(model.SdCardShellTemp.Id);
            ReadingComment readingComment = await Context.ReadingComments.FindAsync(model.Comment.Id);

            if (temp == null || device == null || readingComment == null)
                throw new NullReferenceException("The temperature, device or comment is null");

            // see if the temperature already has a comment
            SdCardShellTemperatureComment exists = await Context.SdCardShellTemperatureComments.FirstOrDefaultAsync(x => x.SdCardShellTemp.Id == temp.Id);
            if (exists != null) // already exists
            {
                exists.Comment = readingComment; // Update the record
            }
            else
            {
                model.SdCardShellTemp = temp;
                model.SdCardShellTemp.Device = device;
                model.Comment = readingComment;
                await Context.SdCardShellTemperatureComments.AddAsync(model);
            }

            await Context.SaveChangesAsync();
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