namespace ShellTemperature.ViewModels.Interfaces
{
    public interface IMeasureSpreadStats
    {
        double Range(double[] values);

        double InterquartileRange(double[] values);

        double InterquartileRange(double[] values,
            out double firstQuartileMedian,
            out double thirdQuartileMedian);

        double StandardDeviation(double[] values);

        double MeanDeviation(double[] values);
    }
}