using System.Drawing;
using OfficeOpenXml.Style;

namespace ExcelDataWriter.Interfaces
{
    public interface IExcelStyler
    {
        void ApplyFontSize(int row, int col, int size);

        void ApplyBorder(int row, int col, ExcelBorderStyle borderStyle, Color color);

        void ApplyBackground(int row, int col, Color color);

        void ApplyForeground(int row, int col, Color color);

        void ApplyFontWeight(int row, int col, bool isBold);
    }
}