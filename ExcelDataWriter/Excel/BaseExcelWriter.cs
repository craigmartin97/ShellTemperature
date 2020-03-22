using ExcelDataWriter.Interfaces;
using OfficeOpenXml.Style;
using System.Diagnostics;
using System.Drawing;

namespace ExcelDataWriter.Excel
{
    public abstract class BaseExcelWriter
    {
        #region Fields

        protected readonly IExcelData _excelData;

        protected readonly IExcelStyler _excelStyler;

        #endregion

        protected BaseExcelWriter(IExcelData excelData, IExcelStyler excelStyler)
        {
            _excelData = excelData;
            _excelStyler = excelStyler;
        }

        public void WriteHeaders(string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
            {
                int col = i + 1;
                _excelData.Worksheet.Cells[1, col].Value = headers[i];
                _excelStyler.ApplyFontSize(1, col, 14);
                _excelStyler.ApplyBackground(1, col, Color.CornflowerBlue);
                _excelStyler.ApplyBorder(1, col, ExcelBorderStyle.Medium, Color.Black);
                _excelStyler.ApplyForeground(1, col, Color.White);
                _excelStyler.ApplyFontWeight(1, col, true);
            }

            _excelData.Package.Save();
        }

        public void OpenFile(string filePath)
        {
            using (Process process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(filePath)
                {
                    UseShellExecute = true
                };
                process.Start();
            }
        }

        protected void AutoFitCols()
        {
            // auto fit columns
            for (int i = 1; i <= _excelData.Worksheet.Dimension.End.Column; i++)
            {
                _excelData.Worksheet.Column(i).AutoFit();
            }
        }
    }
}