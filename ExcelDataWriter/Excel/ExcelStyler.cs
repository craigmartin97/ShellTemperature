using System.Drawing;
using ExcelDataWriter.Interfaces;
using OfficeOpenXml.Style;

namespace ExcelDataWriter.Excel
{
    public class ExcelStyler : IExcelStyler
    {
        private readonly IExcelData _excelData;

        public ExcelStyler(IExcelData excelData)
        {
            _excelData = excelData;
        }

        public void ApplyFontSize(int row, int col, int size)
        {
            _excelData.Worksheet.Cells[row, col].Style.Font.Size = size;
        }

        public void ApplyBorder(int row, int col, ExcelBorderStyle borderStyle, Color color)
        {
            _excelData.Worksheet.Cells[row, col].Style.Border.Right.Style = borderStyle;
            _excelData.Worksheet.Cells[row, col].Style.Border.Left.Style = borderStyle;
            _excelData.Worksheet.Cells[row, col].Style.Border.Top.Style = borderStyle;
            _excelData.Worksheet.Cells[row, col].Style.Border.Bottom.Style = borderStyle;
            _excelData.Worksheet.Cells[row, col].Style.Border.Right.Color.SetColor(color);
            _excelData.Worksheet.Cells[row, col].Style.Border.Left.Color.SetColor(color);
            _excelData.Worksheet.Cells[row, col].Style.Border.Top.Color.SetColor(color);
            _excelData.Worksheet.Cells[row, col].Style.Border.Bottom.Color.SetColor(color);
        }

        public void ApplyBackground(int row, int col, Color color)
        {
            _excelData.Worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            _excelData.Worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor(color);
        }

        public void ApplyForeground(int row, int col, Color color)
        {
            _excelData.Worksheet.Cells[row, col].Style.Font.Color.SetColor(color);
        }

        public void ApplyFontWeight(int row, int col, bool isBold)
        {
            _excelData.Worksheet.Cells[row, col].Style.Font.Bold = isBold;
        }
    }
}