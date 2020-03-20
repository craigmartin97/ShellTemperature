using ShellTemperature.ViewModels.Interfaces;

namespace ShellTemperature.ViewModels.Statistics
{
    public class SortingAlgorithm : ISorter
    {
        public void BubbleSort(double[] values)
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
        }

        private int Partition(double[] values, int low, int high)
        {
            // Last is pivot
            double pivot = values[high];

            int i = low;
            for (int j = low; j < high; j++)
            {
                if (values[j] < pivot) // 2 < 4 yes
                {
                    double temp = values[i]; // 7
                    values[i] = values[j]; // 2
                    values[j] = temp; // 7

                    i++; // 1
                }
            }

            double swap = values[i]; // 8
            values[i] = values[high];
            values[high] = swap;

            return i;
        }

        public void QuickSort(double[] values, int low, int high)
        {
            if (low < high)
            {
                int partitionIndex = Partition(values, low, high);

                QuickSort(values, low, partitionIndex - 1);
                QuickSort(values, partitionIndex + 1, high);
            }
        }
    }
}