using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ShellTemperature.Repository
{
    /// <summary>
    /// Reading comment repository is responsible for interacting with
    /// the reading comments database table
    /// </summary>
    public class ReadingCommentRepository : BaseRepository, IReadingCommentRepository<ReadingComment>
    {
        public ReadingCommentRepository(ShellDb context) : base(context) { }

        /// <summary>
        /// Create a new reading comment
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<bool> Create(ReadingComment model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The model supplied is null");

            // ensure the comment doesn't exist
            ReadingComment exists = GetItem(model.Comment);
            if (exists != null) // not null so it already exists
                return false;

            // exists was null so it can't exist, add into dbo
            await Context.ReadingComments.AddAsync(model);
            await Context.SaveChangesAsync();
            return true;

        }

        #region Get

        /// <summary>
        /// Get all of the reading comments
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ReadingComment> GetAll()
            => Context.ReadingComments;

        /// <summary>
        /// Find a specific reading comment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ReadingComment GetItem(Guid id)
            => Context.ReadingComments.Find(id);

        /// <summary>
        /// Find the comment by the comment string itself
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public ReadingComment GetItem(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentNullException(nameof(comment), "The comment supplied is null");

            ReadingComment readingComment = Context.ReadingComments.FirstOrDefault(x =>
                x.Comment.Equals(comment));

            return readingComment;
        }
        #endregion

        /// <summary>
        /// Delete a specific reading comment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(Guid id)
        {
            ReadingComment readingComment = GetItem(id);
            if(readingComment == null)
                throw new NullReferenceException("Could not find the comment");

            //bool inUse = Context.ShellTemperatureComments.Any(x => x.Comment.Id.Equals(id));
            //if (inUse)
            //    return false; // It is in use can't delete!

            Context.ReadingComments.Remove(readingComment);
            Context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Delete a collection of reading comments
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public bool DeleteRange(IEnumerable<ReadingComment> items)
        {
            if(items == null)
                throw new ArgumentNullException(nameof(items), "The collection supplied was null");

            Context.ReadingComments.RemoveRange(items);
            Context.SaveChanges();
            return true;
        }

        /// <summary>
        /// Update a reading comment
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool Update(ReadingComment model)
        {
            if(model == null)
                throw new ArgumentNullException(nameof(model), "The model supplied is null");

            ReadingComment dbReadingComment = GetItem(model.Id);
            if(dbReadingComment == null)
                throw new NullReferenceException("The reading comment could not be found");

            // Check if the comment already exists as anther entry
            IEnumerable<ReadingComment> allDeviceInfos = GetAll();
            bool alreadyExists = allDeviceInfos.Where(comment => comment.Id != model.Id)
                .Select(comment => comment.Comment.Equals(model.Comment))
                .Any(x => x);

            if (alreadyExists)
                return false; // Can't be updated as it already exists as another entry

            dbReadingComment.Comment = model.Comment;
            Context.SaveChanges();
            return true;
        }
    }
}