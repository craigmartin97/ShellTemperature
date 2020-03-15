using Moq;
using NUnit.Framework;
using ShellTemperature.Data;
using ShellTemperature.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShellTemperature.Tests.RepositoryTests
{
    public class PositionsRepositoryTests : BaseRepositoryTest
    {
        private PositionsRepository positionsRepository;

        private IList<Positions> devPos = new List<Positions>
        {
            new Positions("Top"),
            new Positions("Bottom"),
            new Positions("Side"),
            new Positions("Extra")
        };

        private IList<ShellTemp> shellTemps = new List<ShellTemp>
        {
            new ShellTemp(Guid.NewGuid(), 22.2, DateTime.Now, 54, 22, new DeviceInfo()
            {
                DeviceAddress = "123",
                DeviceName = "Thor"
            }),
            new ShellTemp(Guid.NewGuid(), 28.2, DateTime.Now.AddSeconds(1), 54, 22, new DeviceInfo()
            {
                DeviceAddress = "456",
                DeviceName = "Marvel"
            }),
            new ShellTemp(Guid.NewGuid(), 13.2, DateTime.Now.AddSeconds(2), 54, 22, new DeviceInfo()
            {
                DeviceAddress = "789",
                DeviceName = "Nina"
            })
        };

        [SetUp]
        public void Setup()
        {
            var context = GetShellDb();

            // Add data to shelltemps and shelltemppositions
            for (var index = 0; index < shellTemps.Count; index++)
            {
                var shellTemp = shellTemps[index];
                context.ShellTemperatures.Add(shellTemp);

                ShellTemp dbTemp = context.ShellTemperatures.Find(shellTemp.Id);

                ShellTemperaturePosition shellTemperaturePosition = new ShellTemperaturePosition
                {
                    ShellTemp = dbTemp,
                    Position = devPos[index]
                };
                context.ShellTemperaturePositions.Add(shellTemperaturePosition);
            }

            context.SaveChanges();

            positionsRepository = new PositionsRepository(context);
            context.Positions.Add(devPos[3]); // Extra

        }

        #region Create

        /// <summary>
        /// Creaet a new valid position.
        /// </summary>
        [Test, Order(1)]
        public void Create_Test()
        {
            // Arrange
            Positions devicePosition = new Positions("Craig");
            // Act
            bool created = positionsRepository.Create(devicePosition);
            // Assert
            Assert.IsTrue(created);
        }

        /// <summary>
        /// Execute the create method and should throw exception
        /// </summary>
        [Test, Order(2)]
        public void Create_ArgumentException_Test()
        {
            Positions devicePosition = null;
            Assert.Throws<ArgumentNullException>(delegate { positionsRepository.Create(devicePosition); });
        }

        [Test, Order(3)]
        public void Create_AlreadyExists_Test()
        {
            foreach (var devicePosition in devPos)
            {
                bool created = positionsRepository.Create(devicePosition);
                Assert.IsFalse(created); // already exists!!!
            }
        }

        [Test, Order(4)]
        public void Create_AlreadyExistsPosition_Test()
        {
            Positions position = new Positions("Side");
            bool created = positionsRepository.Create(position);
            Assert.IsFalse(created); // already exists!!!
        }

        #endregion

        #region Get

        /// <summary>
        /// Get all the items
        /// </summary>
        [Test, Order(5)]
        public void GetAll_Test()
        {
            // Act
            IList<Positions> devicePositions = positionsRepository.GetAll().ToList();

            // Assert
            Assert.IsNotNull(devicePositions);
            Assert.IsTrue(devicePositions.Count > 0);
            Assert.IsTrue(devicePositions.Count == 3);

            for (int i = 0; i < devicePositions.Count; i++)
            {
                Assert.AreEqual(devPos[i], devicePositions[i]);
            }
        }

        /// <summary>
        /// Get each position individually
        /// </summary>
        [Test, Order(6)]
        public void GetSinglePosition_Test()
        {
            foreach (Positions position in devPos)
            {
                // Act
                Positions dbDevicePosition = positionsRepository.GetItem(position.Id);

                // Assert
                Assert.IsNotNull(dbDevicePosition);
                Assert.AreEqual(dbDevicePosition, position);
            }
        }

        /// <summary>
        /// Supply an invalid guid id, should return a null response
        /// </summary>
        [Test, Order(7)]
        public void GetSinglePosition_Invalid()
        {
            // Arrange
            Guid id = It.IsAny<Guid>();
            // Act
            Positions position = positionsRepository.GetItem(id);
            // Assert
            Assert.IsNull(position);

        }

        #endregion

        #region Delete

        [Test, Order(8)]
        public void Delete_InUsePositions_Test()
        {
            // First three elements will return false as they are in use by 3 shell temps
            for (int i = 0; i < devPos.Count - 1; i++)
            {
                // Act
                bool cannotDelete = positionsRepository.Delete(devPos[i].Id);

                // Assert
                Assert.IsFalse(cannotDelete); // unable to delete as in use
            }
        }

        [Test, Order(9)]
        public void Delete_NotInUsePositions_Test()
        {
            // ACt
            bool canDelete = positionsRepository.Delete(devPos[3].Id);
            // ASsert
            Assert.IsTrue(canDelete);
        }

        /// <summary>
        /// Supply an invalid id, throws exception
        /// </summary>
        [Test, Order(10)]
        public void Delete_InvalidId_Test()
        {
            // Arrange
            Guid id = It.IsAny<Guid>();

            // ACt and Assert
            Assert.Throws<NullReferenceException>(delegate { positionsRepository.Delete(id); });
        }

        [Test, Order(11)]
        public void DeleteRange_Test()
        {
            // Arrange
            IEnumerable<Positions> validDevicePositions = new List<Positions>()
            {
                devPos[3] // valid one to delete
            };

            // Act
            bool deleted = positionsRepository.DeleteRange(validDevicePositions);

            // Assert
            Assert.IsTrue(deleted);

        }

        #endregion

        #region Update

        [Test, Order(13)]
        public void Update_Test()
        {
            foreach (var devicePosition in devPos)
            {
                string originalPosition = devicePosition.Position;
                // Arrange
                devicePosition.Position += "123"; // Append 123

                // ACt
                bool updated = positionsRepository.Update(devicePosition);

                // ASsert
                Assert.IsTrue(updated);
                Assert.AreEqual(originalPosition + "123", devicePosition.Position);
            }
        }

        [Test, Order(14)]
        public void Update_NullObject_Test()
        {
            Assert.Throws<ArgumentNullException>(delegate { positionsRepository.Update(null); });
        }

        [Test, Order(15)]
        public void Update_NonExistObject()
        {
            // Arrange
            Positions devicePosition = new Positions("NULL");
            devicePosition.Position += "123"; // Edit

            Assert.Throws<NullReferenceException>(delegate { positionsRepository.Update(devicePosition); });
        }

        [Test, Order(16)]
        public void Update_Conflict()
        {
            foreach (var devicePosition in devPos)
            {
                // Will throw issue as position already exists!!!!
                string otherItemsPosition = devPos.Where(x => x.Id != devicePosition.Id)
                    .Select(x => x.Position)
                    .FirstOrDefault();

                devicePosition.Position = otherItemsPosition;
                bool updated = positionsRepository.Update(devicePosition);

                Assert.IsFalse(updated);
            }
        }

        #endregion
    }
}