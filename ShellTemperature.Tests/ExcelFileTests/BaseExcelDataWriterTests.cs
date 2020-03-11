using ExcelDataWriter.Excel;
using ExcelDataWriter.Interfaces;
using NUnit.Framework;
using ShellTemperature.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ShellTemperature.Data;

namespace ShellTemperature.Tests.ExcelFileTests
{
    public class BaseExcelDataWriterTests
    {
        [Test]
        public void WriteHeaders_Test()
        {
            // Arrange
            // path and sheet info for the excel file
            string path = Path.GetTempPath() + Guid.NewGuid() + ".xlsx";
            const string worksheetName = "test";
            IExcelData excelData = new ExcelData(path);
            IExcelStyler excelStyler = new ExcelStyler(excelData);

            excelData.CreateExcelWorkSheet(path, worksheetName);
            bool exists = File.Exists(path);
            Assert.IsTrue(exists);
            Assert.IsTrue(excelData.Package != null);
            Assert.IsTrue(excelData.Worksheet != null);
            Assert.AreEqual(worksheetName, excelData.Worksheet.Name);

            string[] headers = new string[]
            {
                "Marvel", "Captain", "Hulk", "Thor", "IronMan", "X-Men",
                "Spiderman", "Black Widow"
            };
            BaseExcelWriter baseExcelWriter = new ExcelWriter(excelData, excelStyler);
            // Act
            baseExcelWriter.WriteHeaders(headers);

            // Assert
            for (int i = 0; i < headers.Length; i++)
            {
                int col = i + 1;
                Assert.AreEqual(excelData.Worksheet.Cells[1, col].Text, headers[i]);
            }

            excelData.DeleteExcelFile();
            Assert.IsFalse(File.Exists(path));
        }

        [Test]
        public void WriteShellTempsToExcelFile()
        {
            // Arrange
            // path and sheet info for the excel file
            string path = Path.GetTempPath() + Guid.NewGuid() + ".xlsx";
            const string worksheetName = "test";
            IExcelData excelData = new ExcelData(path);
            IExcelStyler excelStyler = new ExcelStyler(excelData);

            excelData.CreateExcelWorkSheet(path, worksheetName);
            bool exists = File.Exists(path);
            Assert.IsTrue(exists);
            Assert.IsTrue(excelData.Package != null);
            Assert.IsTrue(excelData.Worksheet != null);
            Assert.AreEqual(worksheetName, excelData.Worksheet.Name);

            ExcelWriter excelWriter = new ExcelWriter(excelData, excelStyler);

            Random random = new Random();

            ShellTemp[] temps = new ShellTemp[1000];
            for (int i = 0; i < temps.Length; i++)
            {
                float? lat = null;
                float? lon = null;
                DeviceInfo device = new DeviceInfo
                {
                    DeviceAddress = "CraigsAddress",
                    DeviceName = "Spiderman",
                    Id = Guid.NewGuid()
                };

                if (i % 4 == 0)
                {
                    lat = random.Next(0, 1000);
                    lon = random.Next(0, 4000);
                }
                ShellTemp temp = new ShellTemp
                {
                    Id = Guid.NewGuid(),
                    Latitude = lat,
                    Longitude = lon,
                    Temperature = random.Next(20, 80),
                    RecordedDateTime = DateTime.Now,
                    Device = device
                };

                temps[i] = temp;
            }

            // add headers
            string[] headers = temps[0].GetType().GetProperties().Select(x => x.Name).ToArray();
            excelWriter.WriteHeaders(headers);
            excelWriter.WriteToExcelFile(temps);

            Assert.AreEqual(temps.Length, excelData.Worksheet.Dimension.End.Row-1);

            excelData.DeleteExcelFile();
            Assert.IsFalse(File.Exists(path));
        }
    }
}
