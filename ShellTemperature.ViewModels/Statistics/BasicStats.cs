using ShellTemperature.ViewModels.Interfaces;
using System;

namespace ShellTemperature.ViewModels.Statistics
{
    public class BasicStats : IBasicStats
    {
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
                total += values[i];

            double average = total / values.Length;
            return Math.Round(average, 2);
        }
    }
}