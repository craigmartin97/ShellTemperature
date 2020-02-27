namespace ShellTemperature.ViewModels.Interfaces
{
    public interface IBasicStats
    {
        double Minimum(double[] values);

        double Maximum(double[] values);

        double Mean(double[] values);

        double Median(double[] values);

        double Mode(double[] values);
    }
}