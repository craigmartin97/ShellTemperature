namespace ShellTemperature.ViewModels.Interfaces
{
    public interface ISorter
    {
        void BubbleSort(double[] values);

        void QuickSort(double[] values, int low, int high);
    }
}