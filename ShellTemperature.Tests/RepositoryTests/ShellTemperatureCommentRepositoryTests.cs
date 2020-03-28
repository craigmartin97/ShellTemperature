using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ShellTemperature.Data;
using ShellTemperature.Repository;

namespace ShellTemperature.Tests.RepositoryTests
{
    public class ShellTemperatureCommentRepositoryTests : BaseRepositoryTest
    {
        private ShellTemperatureCommentRepository shellTemperatureCommentRepository;

        DeviceInfo[] deviceInfo = new[]
        {
            new DeviceInfo()
            {
                DeviceName = "Marvel",
                DeviceAddress = "Marvel"
            },
            new DeviceInfo()
            {
                DeviceName = "Groot",
                DeviceAddress = "Groot"
            }
        };

        private IList<ShellTemp> shellTemps = new List<ShellTemp>();
        private IList<ReadingComment> readingComments = new List<ReadingComment>();
        private IList<ShellTemperatureComment> shellTemperatureComments = new List<ShellTemperatureComment>();

        [SetUp]
        public void Setup()
        {
            Context = GetShellDb();
            shellTemperatureCommentRepository = new ShellTemperatureCommentRepository(Context);

            Random random = new Random();

            shellTemps.Clear();
            readingComments.Clear();
            shellTemperatureComments.Clear();

            string[] comments = new[]
            {
                "IronMan", "Thor", "CaptainMarvel", "Thanos", "BlackWidow", "Wolverine", "JonSnow", "Deadpool",
                "Rocket", "Quill"
            };

            for (int i = 0; i < 10; i++)
            {
                int temp = random.Next(18, 25);
                int lat = random.Next(0, 55);
                int lon = random.Next(0, 10);

                ShellTemp shellTemp = new ShellTemp(Guid.NewGuid(), temp, DateTime.Now,
                    lat, lon, deviceInfo[random.Next(0, deviceInfo.Length)]);
                shellTemps.Add(shellTemp);

                ReadingComment readingComment = new ReadingComment(comments[i]);
                readingComments.Add(readingComment);

                ShellTemperatureComment shellTemperatureComment =
                    new ShellTemperatureComment(readingComment, shellTemp);
                shellTemperatureComments.Add(shellTemperatureComment);

                Context.ReadingComments.Add(readingComment);
                Context.ShellTemperatures.Add(shellTemp);
                Context.ShellTemperatureComments.Add(shellTemperatureComment);
            }

            Context.SaveChanges();
        }

        #region Get
        [Test, Order(1)]
        public void GetSingleItem()
        {
            foreach (var shellTemperatureComment in shellTemperatureComments)
            {
                ShellTemperatureComment dbComment = shellTemperatureCommentRepository.GetItem(shellTemperatureComment.Id);
                Assert.IsNotNull(dbComment);
                Assert.AreEqual(shellTemperatureComment, dbComment);
                Assert.IsNotNull(dbComment.Comment);
                Assert.IsNotNull(dbComment.ShellTemp);
                Assert.AreEqual(shellTemperatureComment.Comment, dbComment.Comment);
                Assert.AreEqual(shellTemperatureComment.ShellTemp, dbComment.ShellTemp);
            }
        }

        [Test, Order(2)]
        public void GetAll()
        {
            IList<ShellTemperatureComment> dbShellTemperatureComments = shellTemperatureCommentRepository.GetAll().ToList();
            Assert.IsNotNull(shellTemperatureComments);
            Assert.IsTrue(dbShellTemperatureComments.Count() == 10);

            for (int i = 0; i < shellTemperatureComments.Count(); i++)
            {
                Assert.AreEqual(dbShellTemperatureComments[i].Id, shellTemperatureComments[i].Id);
                Assert.AreEqual(dbShellTemperatureComments[i].Comment, shellTemperatureComments[i].Comment);
                Assert.AreEqual(dbShellTemperatureComments[i].ShellTemp, shellTemperatureComments[i].ShellTemp);
            }
        }
        #endregion

        #region Create
        [Test]
        public async void Create()
        {
            // Arrange
            Random random = new Random();

            ShellTemp shellTemp = shellTemps[random.Next(0, shellTemps.Count)];
            ReadingComment readingComment = readingComments[random.Next(0, readingComments.Count)];

            ShellTemperatureComment newComment = new ShellTemperatureComment(readingComment, shellTemp);

            // ACt
            bool created = await shellTemperatureCommentRepository.Create(newComment);

            // Assert
            Assert.IsTrue(created);
        }

        [Test]
        public void Create_BadModel()
        {
            Assert.Throws<ArgumentNullException>(delegate { shellTemperatureCommentRepository.Create(null); });
        }
        #endregion

        #region Delete
        [Test]
        public void Delete()
        {
            for (int i = shellTemperatureComments.Count - 1; i >= 0; i--)
            {
                bool deleted = shellTemperatureCommentRepository.Delete(shellTemperatureComments[i].Id);
                Assert.IsTrue(deleted);

                IList<ShellTemperatureComment> dbComments = shellTemperatureCommentRepository.GetAll().ToList();
                Assert.IsTrue(dbComments.Count == i);
            }
        }

        [Test]
        public void Delete_BadId()
        {
            Guid id = It.IsAny<Guid>();
            Assert.Throws<NullReferenceException>(delegate { shellTemperatureCommentRepository.Delete(id); });
        }

        [Test]
        public void DeleteRange()
        {
            bool deleted = shellTemperatureCommentRepository.DeleteRange(shellTemperatureComments);
            Assert.IsTrue(deleted);
        }

        [Test]
        public void DeleteRange_NullColl()
        {
            Assert.Throws<ArgumentNullException>(delegate { shellTemperatureCommentRepository.DeleteRange(null); });
        }
        #endregion

        #region Update
        [Test]
        public void Update()
        {
            Random random = new Random();
            foreach (var comment in shellTemperatureComments)
            {
                IList<ReadingComment> selections = readingComments.Where(x => x.Id != comment.Comment.Id).ToList();
                ReadingComment newReadingComment = selections[random.Next(0, selections.Count())];

                comment.Comment = newReadingComment;

                bool updated = shellTemperatureCommentRepository.Update(comment);
                Assert.IsTrue(updated);
            }
        }

        [Test]
        public void Update_NullArgs()
        {
            Assert.Throws<ArgumentNullException>(delegate { shellTemperatureCommentRepository.Update(null); });
        }

        [Test]
        public void Update_InvalidId()
        {
            Assert.Throws<NullReferenceException>(delegate
            {
                Guid id = It.IsAny<Guid>();
                Random random = new Random();

                ShellTemperatureComment shellTemperatureComment =
                    shellTemperatureComments[random.Next(0, shellTemperatureComments.Count)];

                // Invalidate guid id
                shellTemperatureComment.Id = id;
                shellTemperatureCommentRepository.Update(shellTemperatureComment); // throws error!!!
            });
        }
        #endregion
    }
}