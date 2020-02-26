using ShellTemperature.Models;

namespace ExcelDataWriter.Interfaces
{
    public interface IExcelWriter<in T>
    {

        void WriteToExcelFile(T temps);
    }
}