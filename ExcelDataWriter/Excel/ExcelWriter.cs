using System.Diagnostics;
using ExcelDataWriter.Interfaces;
using ShellTemperature.Models;

namespace ExcelDataWriter.Excel
{
    /// <summary>
    /// 
    /// </summary>
    public class ExcelWriter : BaseExcelWriter, IExcelWriter<ShellTemp[]>
    {
        public ExcelWriter(IExcelData excelData, IExcelStyler excelStyler) : base(excelData, excelStyler)
        { }

        public void WriteToExcelFile(ShellTemp[] temps)
        {
            if (_excelData.Worksheet.Dimension == null)
            {
                return;
            }

            int row = _excelData.Worksheet.Dimension.Start.Row + 1;

            int id = GetIndexOfColumnHeader(nameof(ShellTemp.Id));
            int temp = GetIndexOfColumnHeader(nameof(ShellTemp.Temperature));
            int dateTime = GetIndexOfColumnHeader(nameof(ShellTemp.RecordedDateTime));
            int latitude = GetIndexOfColumnHeader(nameof(ShellTemp.Latitude));
            int longitude = GetIndexOfColumnHeader(nameof(ShellTemp.Longitude));
            int device = GetIndexOfColumnHeader(nameof(ShellTemp.Device));

            foreach (ShellTemp reading in temps)
            {
                _excelData.Worksheet.Cells[row, id].Value = reading.Id;
                _excelData.Worksheet.Cells[row, temp].Value = reading.Temperature;
                _excelData.Worksheet.Cells[row, dateTime].Value = reading.RecordedDateTime.ToString("dd/MM/yyyy HH:mm:ss");

                _excelData.Worksheet.Cells[row, latitude].Value = reading.Latitude != null
                    ? reading.Latitude.ToString() : "N/A";

                _excelData.Worksheet.Cells[row, longitude].Value = reading.Longitude != null
                    ? reading.Longitude.ToString() : "N/A";

                if (reading.Device != null)
                    _excelData.Worksheet.Cells[row, device].Value = reading.Device.DeviceName;

                row++;
            }

            // auto fit columns
            for (int i = 1; i <= _excelData.Worksheet.Dimension.End.Column; i++)
            {
                _excelData.Worksheet.Column(i).AutoFit();
            }

            _excelData.Package.Save();
        }

        private int GetIndexOfColumnHeader(string columnHeaderName)
        {
            for (int i = 1; i <= _excelData.Worksheet.Dimension.End.Column; i++)
            {
                string currentHeader = _excelData.Worksheet.Cells[1, i].Text;
                if (currentHeader.Equals(columnHeaderName))
                    return i;
            }

            return -1;
        }
    }
}