using NUnit.Framework;
using ShellTemperature.Data;
using ShellTemperature.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using Moq;

namespace ShellTemperature.Tests.RepositoryTests
{
    public class ReadingCommentRepositoryTests : BaseRepositoryTest
    {
        private IList<ReadingComment> readingComments = new List<ReadingComment>
        {
            new ReadingComment("Marvel"),
            new ReadingComment("Spiderman")
        };

        private ReadingCommentRepository readingCommentRepository;

        [SetUp]
        public void Setup()
        {
            Context = GetShellDb();
            readingCommentRepository = new ReadingCommentRepository(Context);

            foreach (var readingComment in readingComments)
            {
                readingCommentRepository.Create(readingComment);
            }
        }

        #region Create

        /// <summary>
        /// Create a new reading comment
        /// </summary>
        [Test, Order(1)]
        public void Create_Test()
        {
            // Arrange
            ReadingComment readingComment = new ReadingComment("MyComment");

            // Act
            bool created = readingCommentRepository.Create(readingComment);

            // Assert
            Assert.IsTrue(created);
        }

        [Test, Order(2)]
        public void Create_NullObject_Test()
        {
            Assert.Throws<ArgumentNullException>(delegate { readingCommentRepository.Create(null); });
        }

        [Test, Order(3)]
        public void Create_AlreadyExists_Test()
        {
            foreach (var readingComment in readingComments)
            {
                bool created = readingCommentRepository.Create(readingComment);
                Assert.IsFalse(created); // they already exists!!!
            }
        }
        #endregion

        #region Get
        [Test, Order(4)]
        public void GetAll_Test()
        {
            IEnumerable<ReadingComment> readingComments = readingCommentRepository.GetAll();

            Assert.IsNotNull(readingComments);
            Assert.IsTrue(readingComments.Any());
            Assert.IsTrue(readingComments.Count() == 2);
        }

        [Test, Order(5)]
        public void GetSingleItem()
        {
            foreach (var readingComment in readingComments)
            {
                ReadingComment dbReadingComment = readingCommentRepository.GetItem(readingComment.Id);
                Assert.IsNotNull(dbReadingComment);
                Assert.AreEqual(readingComment, dbReadingComment);
            }
        }

        /// <summary>
        /// Supply a bad id to the get function. Should return a null response
        /// </summary>
        [Test, Order(6)]
        public void GetSingleItem_InvalidId()
        {
            Guid id = It.IsAny<Guid>();
            ReadingComment dbReadingComment = readingCommentRepository.GetItem(id);
            Assert.IsNull(dbReadingComment);
        }

        /// <summary>
        /// Try and get a comment that doesnt exist
        /// </summary>
        [Test, Order(7)]
        public void GetItem_ByComment()
        {
            // Arrange
            string comment = "MyString";

            // ACt
            ReadingComment readingComment = readingCommentRepository.GetItem(comment);

            //Assert
            Assert.IsNull(readingComment);
        }

        [Test, Order(8)]
        public void GetItem_ByCommentAlreadyExists()
        {
            foreach (var comment in readingComments)
            {
                // ACt
                ReadingComment readingComment = readingCommentRepository.GetItem(comment.Comment);

                //Assert
                Assert.IsNotNull(readingComment);
            }
        }

        [Test, Order(9)]
        public void GetItem_NullAndBlankString()
        {
            Assert.Throws<ArgumentNullException>(delegate { readingCommentRepository.GetItem(null); });
        }
        #endregion

        #region Delete
        [Test, Order(10)]
        public void Delete_Test()
        {
            foreach (var comment in readingComments)
            {
                bool deleted = readingCommentRepository.Delete(comment.Id);
                Assert.IsTrue(deleted);
            }
        }

        [Test, Order(11)]
        public void Delete_InUse_Test()
        {
            // Add comment to to shell temp comments table
            ReadingComment readingComment = readingComments.FirstOrDefault();
            ShellTemp shellTemp = new ShellTemp(Guid.NewGuid(), 22.2, DateTime.Now, 54, 1, new DeviceInfo()
            {
                DeviceName = "Test",
                DeviceAddress = "Test"
            });

            ShellTemperatureComment comment = new ShellTemperatureComment(readingComment, shellTemp);
            Context.ShellTemperatureComments.Add(comment);
            Context.SaveChanges();

            // Delete comment that is in use
            bool deleted = readingCommentRepository.Delete(readingComment.Id);

            Assert.IsFalse(deleted); // Cant delete as in use
        }

        [Test, Order(12)]
        public void DeleteRange_Test()
        {
            bool allDeleted = readingCommentRepository.DeleteRange(readingComments);
            Assert.IsTrue(allDeleted);
        }
        #endregion

        #region Update
        [Test, Order(13)]
        public void Update_AlreadyExists_Test()
        {
            ReadingComment newComment = new ReadingComment("Groot");
            Context.ReadingComments.Add(newComment);

            newComment.Comment = "Marvel";
            bool updated = readingCommentRepository.Update(newComment);
            Assert.IsFalse(updated);
        }

        [Test, Order(14)]
        public void Update_Test()
        {
            foreach (var comment in readingComments)
            {
                comment.Comment += "123";
                bool updated = readingCommentRepository.Update(comment);
                Assert.IsTrue(updated);
            }
        }
        #endregion
    }
}