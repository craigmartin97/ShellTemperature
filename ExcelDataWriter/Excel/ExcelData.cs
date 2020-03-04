using ExcelDataWriter.Interfaces;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace ExcelDataWriter.Excel
{
    public class ExcelData : IExcelData
    {
        #region Fields
        private readonly string file;
        #endregion

        public ExcelWorksheet Worksheet { get; private set; }
        public ExcelPackage Package { get; private set; }

        public ExcelData(string path)
        {
            file = path;
        }

        public ExcelData(string path, string worksheetName)
        {
            file = path;
            OpenExcelFile(path, worksheetName);
        }

        public void CreateExcelWorkSheet(string path, string worksheetName)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                Thread.Sleep(100); // sleep to ensure process complete
            }

            using (ExcelPackage package = new ExcelPackage(new FileInfo(path)))
            {
                package.Workbook.Worksheets.Add(worksheetName);
                package.Save();
            }

            OpenExcelFile(path, worksheetName);
        }

        public void OpenExcelFile(string path, string worksheetName)
        {
            // check that the parameters supplied are valid. If either null or empty then error would occur stop here.
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path), "The path supplied is blank. Enter a valid path");

            if (string.IsNullOrWhiteSpace(worksheetName))
                throw new ArgumentNullException(nameof(worksheetName), "The sheet name supplied is blank. " +
                                                                       "Enter a valid sheet name");

            Package = new ExcelPackage(new FileInfo(path));

            // find the worksheet by name
            Worksheet = Package.Workbook.Worksheets
                .FirstOrDefault(x => x.Name.Equals(worksheetName, StringComparison.CurrentCultureIgnoreCase));

            // throw error if no worksheet has been found
            if (Worksheet == null)
                throw new FileNotFoundException("Could not find the worksheet. Check the file path and worksheet " +
                                                "name are correct and are correct.");
        }

        public bool DeleteExcelFile()
        {
            if (file == null)
                throw new NullReferenceException("The package is null");

            if (File.Exists(file))
            {
                File.Delete(file);
                return true;
            }

            return false;
        }
    }
}