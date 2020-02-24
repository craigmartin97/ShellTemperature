using ShellTemperature.ViewModels.Interfaces;

namespace ShellTemperature.ViewModels.Statistics
{
    public class SortingAlgorithm : ISorter
    {
        public double[] BubbleSort(double[] values)
        {
            for (int write = 0; write < values.Length; write++)
            {
                for (int sort = 0; sort < values.Length - 1; sort++)
                {
                    if (values[sort] > values[sort + 1])
                    {
                        double temp = values[sort + 1];
                        values[sort + 1] = values[sort];
                        values[sort] = temp;
                    }
                }
            }

            return values;
        }
    }
}