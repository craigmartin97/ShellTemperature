using ShellTemperature.ViewModels.Interfaces;
using System;

namespace ShellTemperature.ViewModels.Statistics
{
    public class MeasureSpreadStats : IMeasureSpreadStats
    {
        private readonly ISorter _sortingAlgorithm;
        private readonly IBasicStats _basicStats;

        public MeasureSpreadStats(ISorter sorter, IBasicStats basicStats)
        {
            _sortingAlgorithm = sorter;
            _basicStats = basicStats;
        }

        /// <summary>
        /// Calculates the range for a collection of data
        /// </summary>
        /// <param name="values">Values to calculate the range for</param>
        /// <returns>Returns the range for the collection</returns>
        public double Range(double[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentNullException(nameof(values), "No data available");
            if (values.Length == 1)
                return 0; // Can't calculate range with one value

            _sortingAlgorithm.QuickSort(values, 0, values.Length - 1);
            double min = values[0];
            double max = values[^1];

            return max - min;
        }

        public double InterquartileRange(double[] values)
        {
            GetQuartiles(values, out double[] firstQuartile, out double[] thirdQuartile);

            if (firstQuartile == null)
                throw new ArgumentNullException(nameof(firstQuartile), "The first quartile is null");

            if (thirdQuartile == null)
                throw new ArgumentNullException(nameof(thirdQuartile), "The third quartile is null");

            double firstQuartileMedian = GetQuantileMedian(firstQuartile);
            double thirdQuartileMedian = GetQuantileMedian(thirdQuartile);

            double interQuartileRange = thirdQuartileMedian - firstQuartileMedian;
            return interQuartileRange;
        }

        /// <summary>
        /// Get the inter-quartile range for the set of data
        /// </summary>
        /// <param name="values">The values to get the inter-quartile range for</param>
        /// <param name="firstQuartileMedian"></param>
        /// <param name="thirdQuartileMedian"></param>
        /// <returns>Returns the inter-quartile range</returns>
        public double InterquartileRange(double[] values, out double firstQuartileMedian,
            out double thirdQuartileMedian)
        {
            GetQuartiles(values, out double[] firstQuartile, out double[] thirdQuartile);

            if (firstQuartile == null)
                throw new ArgumentNullException(nameof(firstQuartile), "The first quartile is null");

            if (thirdQuartile == null)
                throw new ArgumentNullException(nameof(thirdQuartile), "The third quartile is null");

            firstQuartileMedian = GetQuantileMedian(firstQuartile);
            thirdQuartileMedian = GetQuantileMedian(thirdQuartile);

            double interQuartileRange = thirdQuartileMedian - firstQuartileMedian;
            return interQuartileRange;
        }

        /// <summary>
        /// Get the first and third quartile arrays
        /// </summary>
        /// <param name="values"></param>
        /// <param name="first"></param>
        /// <param name="third"></param>
        public void GetQuartiles(double[] values,
            out double[] first,
            out double[] third)
        {
            _sortingAlgorithm.BubbleSort(values);

            double[] firstQuartile;
            double[] thirdQuartile;

            int index;
            int thirdQuantileStartIndex;
            // is even
            if (values.Length % 2 == 0)
            {
                index = values.Length / 2;
                thirdQuantileStartIndex = index;
            }
            else // is odd
            {
                index = (values.Length - 1) / 2;
                thirdQuantileStartIndex = index + 1;
            }

            firstQuartile = new double[index];
            thirdQuartile = new double[index];

            for (int i = 0; i < index; i++)
            {
                firstQuartile[i] = values[i];
            }

            int indexCounter = 0;
            for (int i = thirdQuantileStartIndex; i < values.Length; i++)
            {
                thirdQuartile[indexCounter] = values[i];
                indexCounter++;
            }

            first = firstQuartile;
            third = thirdQuartile;
        }

        /// <summary>
        /// Get the quartile median.
        /// Helper function for the interquartile range
        /// </summary>
        /// <param name="quantile">The quantile to get the median for</param>
        /// <returns>Returns the median of a quantile set of data</returns>
        private double GetQuantileMedian(double[] quantile)
        {
            if (quantile == null) throw new ArgumentNullException(nameof(quantile));
            if (quantile.Length < 2) return 0; // can't calculate with not enough values

            if (quantile.Length % 2 == 0) // even
            {
                int startIndex = (quantile.Length / 2) == 1 ? (quantile.Length / 2) - 1 : (quantile.Length / 2);

                int endIndex = startIndex + 1;

                return (quantile[startIndex] + quantile[endIndex]) / 2;
            }
            else // odd
            {
                //int medianIndex = ((quantile.Length + 1) / 2) - 1;
                int medianIndex = (quantile.Length - 1) / 2;
                return quantile[medianIndex];
            }
        }

        /// <summary>
        /// Calculate the standard deviation for a set of data
        /// </summary>
        /// <param name="values">The values to calculate the standard deviation for</param>
        /// <returns>Returns the standard deviation for the set of data</returns>
        public double StandardDeviation(double[] values)
        {
            // calculate mean average
            double meanAvg = _basicStats.Mean(values);

            // each points difference to the mean
            double[] variance = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                double pointDiff = values[i] - meanAvg;
                double pointVariance = pointDiff * pointDiff; // squared
                variance[i] = pointVariance;
            }

            // total variance
            double total = 0;
            for (int i = 0; i < variance.Length; i++)
                total += variance[i];

            double totalVariance = ((double)1 / variance.Length) * total;
            double standardDeviation = Math.Sqrt(totalVariance);
            return Math.Round(standardDeviation, 2);
        }

        /// <summary>
        /// Calculate the mean deviation for a collection of data
        /// </summary>
        /// <param name="values">The values to calculate the mean deviation for</param>
        /// <returns>Returns the mean deviation</returns>
        public double MeanDeviation(double[] values)
        {
            double meanAvg = _basicStats.Mean(values);

            double[] distances = new double[values.Length];
            for (int i = 0; i < values.Length; i++)
            {
                double distance = Math.Abs(meanAvg - values[i]);
                distances[i] = distance;
            }

            double meanOfDistances = _basicStats.Mean(distances);
            return Math.Round(meanOfDistances, 2);
        }
    }
}