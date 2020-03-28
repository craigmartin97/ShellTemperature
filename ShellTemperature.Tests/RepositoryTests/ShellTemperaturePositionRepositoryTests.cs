using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using ShellTemperature.Data;
using ShellTemperature.Repository;

namespace ShellTemperature.Tests.RepositoryTests
{
    public class ShellTemperaturePositionRepositoryTests : BaseRepositoryTest
    {

        private DeviceInfo[] deviceInfos = new[]
        {
            new DeviceInfo()
            {
                DeviceName = "Marvel",
                DeviceAddress = "Marvel"
            },
            new DeviceInfo()
            {
                DeviceName = "Spiderman",
                DeviceAddress = "Spiderman"
            }
        };
        private IList<ShellTemp> shellTemps = new List<ShellTemp>();
        private IList<Positions> positions = new List<Positions>();
        private IList<ShellTemperaturePosition> shellTemperaturePositions = new List<ShellTemperaturePosition>();

        private ShellTemperaturePositionRepository shellTemperaturePositionRepository;

        [SetUp]
        public void Setup()
        {
            Context = GetShellDb();
            shellTemperaturePositionRepository = new ShellTemperaturePositionRepository(Context);

            shellTemps.Clear();
            positions.Clear();
            shellTemperaturePositions.Clear();

            Random random = new Random();

            string[] pos = new[]
            {
                "Ironman", "Thor", "StarLord", "Rocket", "Gamora", "Thanos",
                "Quill", "Groot", "BlackWidow", "Hawkeye"
            };

            Context.DevicesInfo.AddRange(deviceInfos);

            for (int i = 0; i < 10; i++)
            {
                int temp = random.Next(18, 25);
                int lat = random.Next(0, 54);
                int lon = random.Next(0, 10);
                DeviceInfo deviceInfo = deviceInfos[random.Next(0, deviceInfos.Length)];

                ShellTemp shellTemp = new ShellTemp(Guid.NewGuid(), temp, DateTime.Now,
                    lat, lon, deviceInfo);

                shellTemps.Add(shellTemp);

                Positions position = new Positions(pos[i]);
                positions.Add(position);

                ShellTemperaturePosition shellTemperaturePosition = new ShellTemperaturePosition
                {
                    ShellTemp = shellTemp,
                    Position = position
                };
                shellTemperaturePositions.Add(shellTemperaturePosition);

                Context.Positions.Add(position);
                Context.ShellTemperatures.Add(shellTemp);
                Context.ShellTemperaturePositions.Add(shellTemperaturePosition);
            }

            Context.SaveChanges();
        }


        [Test]
        public async void Create()
        {
            Random random = new Random();

            int temp = random.Next(18, 25);
            int lat = random.Next(0, 54);
            int lon = random.Next(0, 10);
            DeviceInfo deviceInfo = deviceInfos[random.Next(0, deviceInfos.Length)];

            ShellTemp shellTemp = new ShellTemp(Guid.NewGuid(), temp, DateTime.Now,
                lat, lon, deviceInfo);

            Positions position = positions[random.Next(0, positions.Count)];

            Context.ShellTemperatures.Add(shellTemp);

            ShellTemperaturePosition shellTemperaturePosition = new ShellTemperaturePosition
            {
                ShellTemp = shellTemp,
                Position = position
            };

            bool created = await shellTemperaturePositionRepository.Create(shellTemperaturePosition);
            Assert.IsTrue(created);
        }

        [Test]
        public void Create_NullModel()
        {
            Assert.Throws<ArgumentNullException>(delegate { shellTemperaturePositionRepository.Create(null); });
        }

        [Test]
        public void Create_DeviceNull()
        {
            Assert.Throws<ArgumentNullException>(delegate
            {
                ShellTemperaturePosition shellTemperaturePosition = new ShellTemperaturePosition();
                shellTemperaturePositionRepository.Create(shellTemperaturePosition);
            });
        }

        [Test]
        public async void Create_CantFindShellTemp()
        {
            Random random = new Random();

            int temp = random.Next(18, 25);
            int lat = random.Next(0, 54);
            int lon = random.Next(0, 10);
            DeviceInfo deviceInfo = deviceInfos[random.Next(0, deviceInfos.Length)];

            ShellTemp shellTemp = new ShellTemp(Guid.NewGuid(), temp, DateTime.Now,
                lat, lon, deviceInfo);

            Positions position = positions[random.Next(0, positions.Count)];

            ShellTemperaturePosition shellTemperaturePosition = new ShellTemperaturePosition
            {
                ShellTemp = shellTemp,
                Position = position
            };

            bool created = await shellTemperaturePositionRepository.Create(shellTemperaturePosition);
            Assert.IsFalse(created);
        }

        [Test]
        public void GetAll()
        {
            IList<ShellTemperaturePosition> shellTempPos =
                shellTemperaturePositionRepository.GetAll().ToList();

            Assert.IsNotNull(shellTempPos);

            for (int i = 0; i < shellTempPos.Count; i++)
            {
                Assert.IsNotNull(shellTempPos[i].Position);
                Assert.IsNotNull(shellTempPos[i].ShellTemp);
                Assert.AreEqual(shellTempPos[i], shellTemperaturePositions[i]);
                Assert.AreEqual(shellTempPos[i].Position, shellTemperaturePositions[i].Position);
                Assert.AreEqual(shellTempPos[i].ShellTemp, shellTemperaturePositions[i].ShellTemp);
                Assert.AreEqual(shellTempPos[i].Id, shellTemperaturePositions[i].Id);
            }
        }

        [Test]
        public void GetItem()
        {
            foreach (var shellTemperaturePosition in shellTemperaturePositions)
            {
                ShellTemperaturePosition temp = shellTemperaturePositionRepository.GetItem(shellTemperaturePosition.Id);
                Assert.IsNotNull(temp);
                Assert.IsNotNull(temp.Position);
                Assert.IsNotNull(temp.ShellTemp);
            }
        }

        [Test]
        public void Delete()
        {
            foreach (var shellTemperaturePosition in shellTemperaturePositions)
            {
                bool deleted = shellTemperaturePositionRepository.Delete(shellTemperaturePosition.Id);
                Assert.IsTrue(deleted);
            }
        }

        [Test]
        public void Delete_BadId()
        {
            Assert.Throws<NullReferenceException>(delegate
            {
                Guid id = It.IsAny<Guid>();
                bool deleted = shellTemperaturePositionRepository.Delete(id);
                Assert.IsFalse(deleted);
            });
        }

        [Test]
        public void Delete_Range()
        {
            bool deleted = shellTemperaturePositionRepository.DeleteRange(shellTemperaturePositions);
            Assert.IsTrue(deleted);
        }

        [Test]
        public void Delete_RangeNullColl()
        {
            Assert.Throws<ArgumentNullException>(delegate { shellTemperaturePositionRepository.DeleteRange(null); });
        }

        [Test]
        public void Update()
        {
            Random random = new Random();
            foreach (var shellTemperaturePosition in shellTemperaturePositions)
            {
                IList<ShellTemp> selections = shellTemps
                    .Where(x => x.Id != shellTemperaturePosition.ShellTemp.Id)
                    .ToList();

                shellTemperaturePosition.ShellTemp = selections[random.Next(0, selections.Count)];
                bool updated = shellTemperaturePositionRepository.Update(shellTemperaturePosition);
                Assert.IsTrue(updated);
            }
        }
    }
}