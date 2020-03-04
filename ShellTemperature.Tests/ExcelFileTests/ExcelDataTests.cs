using ExcelDataWriter.Excel;
using ExcelDataWriter.Interfaces;
using NUnit.Framework;
using System;
using System.IO;

namespace ShellTemperature.Tests.ExcelFileTests
{
    public class ExcelDataTests
    {
        /// <summary>
        /// Create a new excel worksheet that doesnt exist
        /// check that it has been created successfully
        /// and then delete it as to save space on disk
        /// </summary>
        [Test]
        public void CreateExcelFile_BlankCtor_Test()
        {
            // Arrange
            // path and sheet info for the excel file
            string path = Path.GetTempPath() + Guid.NewGuid() + ".xlsx";
            const string worksheetName = "test";

            IExcelData excelData = new ExcelData(path);

            // Act
            excelData.CreateExcelWorkSheet(path, worksheetName);

            // Assert
            bool exists = File.Exists(path);
            Assert.IsTrue(exists);
            Assert.IsTrue(excelData.Package != null);
            Assert.IsTrue(excelData.Worksheet != null);
            Assert.AreEqual(worksheetName, excelData.Worksheet.Name);

            // Now delete the excel file, as to not create hundreds of them on disk
            excelData.DeleteExcelFile();
            bool noLongerExists = File.Exists(path);
            Assert.IsFalse(noLongerExists);
        }
    }
}
