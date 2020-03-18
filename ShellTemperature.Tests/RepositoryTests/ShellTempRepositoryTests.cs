using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using ShellTemperature.Data;
using ShellTemperature.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShellTemperature.Tests.RepositoryTests
{
    public class ShellTempRepositoryTests : BaseRepositoryTest
    {
        private ShellTemperatureRepository temperatureRepository;
        private IList<ShellTemp> temps = new List<ShellTemp>
        {
            new ShellTemp
            {
                Id = Guid.NewGuid(),
                Latitude = 1000,
                Longitude = 1000,
                RecordedDateTime = DateTime.Now.AddDays(-1),
                Temperature = 20.2,
                Device = new DeviceInfo
                {
                    Id = Guid.NewGuid(),
                    DeviceAddress = "Spiderman",
                    DeviceName = "Hulk"
                }
            },
            new ShellTemp
            {
                Id = Guid.NewGuid(),
                Latitude = 7000,
                Longitude = 9000,
                RecordedDateTime = DateTime.Now.AddDays(-1),
                Temperature = 23.2,
                Device = new DeviceInfo
                {
                    Id = Guid.NewGuid(),
                    DeviceAddress = "IronMan",
                    DeviceName = "CaptainAmerica"
                }
            },
            new ShellTemp
            {
                Id = Guid.NewGuid(),
                Latitude = 7000,
                Longitude = 9000,
                RecordedDateTime = DateTime.Now.AddDays(-16),
                Temperature = 27.8,
                Device = new DeviceInfo
                {
                    Id = Guid.NewGuid(),
                    DeviceAddress = "Thor",
                    DeviceName = "SheHulk"
                }
            }
        };

        [SetUp]
        public void Setup()
        {
            temperatureRepository = new ShellTemperatureRepository(GetShellDb());
            foreach (ShellTemp temp in temps)
            {
                temperatureRepository.Create(temp);
            }
        }

        [Test, Order(1)]
        public void Create_Test()
        {
            // Arrange
            Random random = new Random();

            for (int i = 0; i < 20; i++)
            {
                ShellTemp temp = new ShellTemp
                {
                    Id = Guid.NewGuid(),
                    Latitude = random.Next(0, 10000),
                    Longitude = random.Next(0, 10000),
                    RecordedDateTime = DateTime.Now,
                    Temperature = random.Next(0, 30),
                    Device = new DeviceInfo
                    {
                        Id = Guid.NewGuid(),
                        DeviceAddress = "Eleven",
                        DeviceName = "Strokes"
                    }
                };

                // Act
                bool created = temperatureRepository.Create(temp);

                // Assert
                Assert.IsTrue(created);
            }
        }

        [Test, Order(2)]
        public void Create_NullDevice_Test()
        {
            // Arrange
            Random random = new Random();

            for (int i = 0; i < 20; i++)
            {
                ShellTemp temp = new ShellTemp
                {
                    Id = Guid.NewGuid(),
                    Latitude = random.Next(0, 10000),
                    Longitude = random.Next(0, 10000),
                    RecordedDateTime = DateTime.Now,
                    Temperature = random.Next(0, 30),
                    Device = null
                };

                Assert.Throws<ArgumentNullException>(delegate
                {
                    temperatureRepository.Create(temp);
                });
            }
        }

        /// <summary>
        /// Get all the items from the database and
        /// compare them to the in memory collection
        /// </summary>
        [Test, Order(3)]
        public void GetAll_Test()
        {
            // Act
            IList<ShellTemp> shellTemps = temperatureRepository.GetAll().ToList();

            // Assert
            Assert.IsNotNull(shellTemps);

            for (int i = 0; i < temps.Count; i++)
            {
                Assert.AreEqual(temps[i], shellTemps[i]);
            }
        }

        /// <summary>
        /// Get all the temperatures between 
        /// a start and end date. Only two should be returned in this
        /// collection and one should be missed out
        /// </summary>
        [Test, Order(4)]
        public void GetAllDataBetweenTwoDates()
        {
            // Arrange
            DateTime start = DateTime.Now.Date.AddDays(-1);
            DateTime end = DateTime.Now.Date;

            // Act
            IList<ShellTemp> shellTemps = temperatureRepository.GetShellTemperatureData(start, end).ToList();

            // Arrange
            Assert.IsTrue(shellTemps.Count == 2);
            Assert.AreNotEqual(temps.Count, shellTemps.Count);
        }

        [Test]
        public void GetSingle()
        {
            foreach (var shellTemp in temps)
            {
                ShellTemp dbShellTemp = temperatureRepository.GetItem(shellTemp.Id);
                Assert.IsNotNull(dbShellTemp);
            }
        }

        /// <summary>
        /// Delete all the items from the database
        /// </summary>
        [Test, Order(5)]
        public void Delete_Test()
        {
            foreach (ShellTemp temp in temps)
            {
                bool deleted = temperatureRepository.Delete(temp.Id);
                Assert.IsTrue(deleted);
            }

            IEnumerable<ShellTemp> shellTemps = temperatureRepository.GetAll();
            Assert.IsTrue(shellTemps.Count() == 0);
        }

        /// <summary>
        /// Delete all the items from the database
        /// </summary>
        [Test, Order(6)]
        public void Delete_BadId_Test()
        {
            Guid randId = Guid.NewGuid();
            bool deleted = temperatureRepository.Delete(randId);
            Assert.IsFalse(deleted);
        }

        /// <summary>
        /// Delete all the items from the database
        /// </summary>
        [Test, Order(7)]
        public void DeleteRange_Test()
        {
            bool deleted = temperatureRepository.DeleteRange(temps);
            Assert.IsTrue(deleted);

            IEnumerable<ShellTemp> shellTemps = temperatureRepository.GetAll();
            Assert.IsTrue(shellTemps.Count() == 0);
        }

        /// <summary>
        /// Delete all the items from the database
        /// </summary>
        [Test, Order(8)]
        public void DeleteRange_Null_Test()
        {
            bool deleted = temperatureRepository.DeleteRange(null);
            Assert.IsFalse(deleted);
        }

        [Test]
        public void Update()
        {
            Random random = new Random();
            foreach (var shellTemp in temps)
            {
                shellTemp.Temperature = random.Next(20, 30); // EDit temp
                bool updated = temperatureRepository.Update(shellTemp);
                Assert.IsTrue(updated);
            }
        }
    }
}
