using OfficeOpenXml;

namespace ExcelDataWriter.Interfaces
{
    public interface IExcelData
    {
        /// <summary>
        /// Worksheet to use
        /// </summary>
        ExcelWorksheet Worksheet { get; }

        /// <summary>
        /// The package the worksheet exists in
        /// </summary>
        ExcelPackage Package { get; }

        void CreateExcelWorkSheet(string path, string worksheetName);

        void OpenExcelFile(string path, string worksheetName);
    }
}