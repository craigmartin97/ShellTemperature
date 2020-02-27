using ShellTemperature.ViewModels.Interfaces;
using System;
using System.Diagnostics;
using System.Linq;

namespace ShellTemperature.ViewModels.Statistics
{
    public class BasicStats : IBasicStats
    {
        private readonly ISorter _sorter;

        public BasicStats(ISorter sorter)
        {
            _sorter = sorter;
        }

        /// <summary>
        /// Get the minimum value from a collection
        /// </summary>
        /// <param name="values">Collection of values</param>
        /// <returns>Returns the minimum value from the collection</returns>
        public double Minimum(double[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentNullException(nameof(values), "No data available");

            double tempMin = values[0];
            for (int i = 1; i < values.Length; i++)
            {
                if (values[i] < tempMin)
                    tempMin = values[i]; // found new smaller number
            }

            return tempMin;
        }

        /// <summary>
        /// Get the maximum value from the collection
        /// </summary>
        /// <param name="values">Collection of values to get the max from</param>
        /// <returns>Returns the maximum from the collection</returns>
        public double Maximum(double[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentNullException(nameof(values), "No data available");

            double tempMax = values[0];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > tempMax)
                    tempMax = values[i]; // found new max temperature
            }

            return tempMax;
        }

        /// <summary>
        /// Calculate the mean average from a collection of doubles
        /// </summary>
        /// <param name="values">Collection of doubles</param>
        /// <returns>Returns a double which is the mean average for the collection</returns>
        public double Mean(double[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentNullException(nameof(values), "No data available");

            double total = 0;
            for (int i = 0; i < values.Length; i++)
            {
                total += values[i];
            }

            double average = total / values.Length;
            return Math.Round(average, 2);
        }

        /// <summary>
        /// Find the mode of the data set
        /// </summary>
        /// <param name="values">The values to find the mode for</param>
        /// <returns>Returns the most often occuring number in the data set</returns>
        public double Mode(double[] values)
        {
            return values.GroupBy(v => v)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;
        }

        /// <summary>
        /// Find the median in the data set
        /// </summary>
        /// <param name="values">The values to find the median for</param>
        /// <returns>Returns the median value of the data set</returns>
        public double Median(double[] values)
        {
            double[] orderedValues = _sorter.BubbleSort(values);

            if (orderedValues.Length % 2 == 0) // even
            {
                int startIndex = (orderedValues.Length / 2) - 1;
                int endIndex = startIndex + 1;

                return (orderedValues[startIndex] + orderedValues[endIndex]) / 2;
            }
            else // odd
            {
                int medianIndex = ((orderedValues.Length + 1) / 2) - 1;
                return orderedValues[medianIndex];
            }
        }
    }
}