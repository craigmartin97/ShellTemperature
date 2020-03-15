using Moq;
using NUnit.Framework;
using ShellTemperature.Data;
using ShellTemperature.Repository;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ShellTemperature.Tests.RepositoryTests
{
    public class DeviceRepositoryTests : BaseRepositoryTest
    {
        private DevicesRepository deviceRepository;
        private IList<DeviceInfo> devices = new List<DeviceInfo>
        {
            new DeviceInfo
            {
                Id = Guid.NewGuid(),
                DeviceAddress = "Spiderman",
                DeviceName = "Marvel"
            },
            new DeviceInfo
            {
                Id = Guid.NewGuid(),
                DeviceAddress = "Hulk",
                DeviceName = "IronMan"
            },
        };

        [SetUp]
        public void Setup()
        {
            Context = GetShellDb();
            deviceRepository = new DevicesRepository(Context);
            foreach (DeviceInfo temp in devices)
            {
                deviceRepository.Create(temp);
            }
        }

        /// <summary>
        /// Create a new device info should pass
        /// </summary>
        [Test, Order(1)]
        public void Create_Test()
        {
            // Arrange
            DeviceInfo deviceInfo = new DeviceInfo
            {
                Id = Guid.NewGuid(),
                DeviceAddress = "SheHulk",
                DeviceName = "JonSnow"
            };

            // Act
            bool created = deviceRepository.Create(deviceInfo);

            // Assert
            Assert.IsTrue(created);
        }

        /// <summary>
        /// Create a new device info should pass
        /// </summary>
        [Test, Order(2)]
        public void Create_NotFound_Test()
        {
            // Arrange
            DeviceInfo deviceInfo = new DeviceInfo
            {
                Id = Guid.NewGuid(),
                DeviceAddress = "Hulk", // Hulk already exists
                DeviceName = "JonSnow"
            };

            // Act
            Assert.Throws<ArgumentException>(delegate
            {
                deviceRepository.Create(deviceInfo);
            });
        }

        /// <summary>
        /// Get all the devices from the SQL database
        /// and compare with the inmemory data set
        /// </summary>
        [Test, Order(3)]
        public void GetAll_Test()
        {
            // Act
            IList<DeviceInfo> devs = deviceRepository.GetAll().ToList();
            Assert.IsNotNull(devs);
            Assert.IsTrue(devs.Count == 2);

            for (int i = 0; i < devices.Count; i++)
            {
                Assert.AreEqual(devs[i], devices[i]);
            }
        }

        /// <summary>
        /// Delete each item from the in memory database
        /// </summary>
        [Test, Order(4)]
        public void Delete_Test()
        {
            foreach (DeviceInfo deviceInfo in devices)
            {
                bool deleted = deviceRepository.Delete(deviceInfo.Id);
                Assert.IsTrue(deleted);
            }
        }

        /// <summary>
        /// Try and delete an item with a bad id value
        /// </summary>
        [Test, Order(5)]
        public void Delete_BadId_Test()
        {
            // Arrange
            Guid id = Guid.NewGuid(); // bad id

            // Act
            bool deleted = deviceRepository.Delete(id);

            // Assert
            Assert.IsFalse(deleted);
        }

        /// <summary>
        /// Delete all the items from the repo
        /// </summary>
        [Test, Order(6)]
        public void DeleteRange_Test()
        {
            // Act
            bool deleted = deviceRepository.DeleteRange(devices);

            // Assert
            Assert.IsTrue(deleted);
        }

        /// <summary>
        /// Try and delete a range from the database which is null and will fail
        /// </summary>
        [Test, Order(7)]
        public void DeleteRange_Fail_Test()
        {
            Assert.Throws<ArgumentNullException>(delegate { deviceRepository.DeleteRange(null); });
        }

        [Test, Order(8)]
        public void Delete_InUse()
        {
            DeviceInfo deviceInfo = devices.FirstOrDefault();
            Assert.IsNotNull(deviceInfo);

            // Add in a new shell temp to the db for testing
            ShellTemp shellTemp = new ShellTemp(Guid.NewGuid(), 22.2, DateTime.Now, 54, 1, deviceInfo);
            Context.ShellTemperatures.Add(shellTemp);
            Context.SaveChanges();

            bool deleted = deviceRepository.Delete(deviceInfo.Id);
            Assert.IsFalse(deleted);
        }

        /// <summary>
        /// Get a single device from the database based on the device address
        /// </summary>
        [Test, Order(9)]
        public void GetSingleDeviceByAddress_Test()
        {
            // Act & Assert
            foreach (DeviceInfo dev in devices)
            {
                DeviceInfo deviceInfo = deviceRepository.GetDevice(dev.DeviceAddress);
                Assert.IsNotNull(deviceInfo);
            }
        }

        /// <summary>
        /// Supply a bad address will retur a ull object
        /// </summary>
        [Test, Order(10)]
        public void GetSingleDeviceByAddressWillFail_Test()
        {
            // Act & Assert
            DeviceInfo deviceInfo = deviceRepository.GetDevice(It.IsAny<string>());
            Assert.IsNull(deviceInfo);
        }

        /// <summary>
        /// Get a single device from the database based upon the id value
        /// </summary>
        [Test, Order(11)]
        public void GetSingleDeviceFromId()
        {
            foreach (DeviceInfo dev in devices)
            {
                DeviceInfo deviceInfo = deviceRepository.GetItem(dev.Id);
                Assert.IsNotNull(deviceInfo);
                Assert.AreEqual(dev, deviceInfo);
            }
        }

        [Test, Order(12)]
        public void GetSingleDeviceById_BadId()
        {
            DeviceInfo deviceInfo = deviceRepository.GetItem(Guid.NewGuid());
            Assert.IsNull(deviceInfo);
        }

        #region Update
        [Test, Order(13)]
        public void Update_Test()
        {
            foreach (var device in devices)
            {
                // Arrange
                device.DeviceAddress += "123";

                // Act
                bool updated = deviceRepository.Update(device);

                // Assert
                Assert.IsTrue(updated);

                DeviceInfo updateDevice = deviceRepository.GetItem(device.Id);
                Assert.AreEqual(device, updateDevice);
            }
        }

        [Test, Order(14)]
        public void Update_NullObject_Test()
        {
            Assert.Throws<ArgumentNullException>(delegate { deviceRepository.Update(null); });
        }

        [Test, Order(15)]
        public void Update_NoneExistsObject_Test()
        {
            // Arrange
            DeviceInfo deviceInfo = new DeviceInfo()
            {
                DeviceName = "NULL",
                DeviceAddress = "NULL"
            };

            // ACt & Assert
            // Should throw as none exists
            Assert.Throws<NullReferenceException>(delegate { deviceRepository.Update(deviceInfo); });
        }
        #endregion
    }
}
