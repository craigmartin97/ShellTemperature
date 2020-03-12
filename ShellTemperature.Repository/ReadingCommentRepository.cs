using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShellTemperature.Repository
{
    public class ReadingCommentRepository : IReadingCommentRepository<ReadingComment>
    {
        private readonly ShellDb _context;

        public ReadingCommentRepository(ShellDb context)
        {
            _context = context;
        }

        public bool Create(ReadingComment model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model), "The model supplied is null");

            // ensure the comment doesn't exist
            ReadingComment exists = GetItem(model.Comment);
            if (exists != null) // not null so it already exists
                return false;

            // exists was null so it can't exist, add into dbo
            _context.ReadingComments.Add(model);
            _context.SaveChanges();
            return true;

        }

        #region Get

        public IEnumerable<ReadingComment> GetAll()
            => _context.ReadingComments;

        public ReadingComment GetItem(Guid id)
            => _context.ReadingComments.Find(id);

        /// <summary>
        /// Find the comment by the comment string itself
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        public ReadingComment GetItem(string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentNullException(nameof(comment), "The comment supplied is null");

            ReadingComment readingComment = _context.ReadingComments.FirstOrDefault(x =>
                x.Comment.Equals(comment));

            return readingComment;
        }
        #endregion

        public bool Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public bool DeleteRange(IEnumerable<ReadingComment> items)
        {
            throw new NotImplementedException();
        }

        public bool Update(ReadingComment model)
        {
            throw new NotImplementedException();
        }
    }
}