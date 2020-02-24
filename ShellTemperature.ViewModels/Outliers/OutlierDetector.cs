using System;
using System.Collections.Generic;
using System.Linq;

namespace ShellTemperature.ViewModels.Outliers
{
    public class OutlierDetector
    {
        public bool IsOutlier(double latestReading, double outlierWeight, IList<double> temps)
        {
            if (temps.Count <= 1) return latestReading < 5; // no prev data yet

            if (latestReading > 10) return false; // can't be an outlier


            IEnumerable<double> lastTen = temps.TakeLast(10);
            double lastTenReadingsAverage = Math.Round(lastTen.Average(), 2);

            return Math.Abs(lastTenReadingsAverage - latestReading) >= outlierWeight;
        }

        public bool IsOutlier(IList<double> temps, double latestReading)
        {
            if (temps.Count == 0) return true;
            if (temps.Count <= 4)
            {
                double latestTemp = temps[^1];
                if (latestTemp <= 10)
                {
                    temps.Remove(latestTemp);
                }

                return temps[^1] < 5;
            }

            double[] sortedTemps = temps.TakeLast(10).OrderBy(x => x).ToArray();

            //double min = sortedTemps[0];
            //double max = sortedTemps[^1];

            double[] firstQuartile;
            double[] thirdQuartile;

            int index;
            // is even
            if (sortedTemps.Length % 2 == 0)
            {
                index = sortedTemps.Length / 2;
                firstQuartile = new double[index];
                thirdQuartile = new double[index];
            }
            else // is odd
            {
                index = ((sortedTemps.Length + 1) / 2) - 1;
                int size = sortedTemps.Length - index;
                firstQuartile = new double[size];
                thirdQuartile = new double[size];
            }

            for (int i = 0; i < index; i++)
            {
                firstQuartile[i] = sortedTemps[i];
            }

            int indexCounter = 0;
            for (int i = index; i < sortedTemps.Length; i++)
            {
                thirdQuartile[indexCounter] = sortedTemps[i];
                indexCounter++;
            }

            double firstQuartileMedian = GetQuantileMedian(firstQuartile);
            double thirdQuartileMedian = GetQuantileMedian(thirdQuartile);

            double interQuartileRange = thirdQuartileMedian - firstQuartileMedian;
            if (Math.Abs(interQuartileRange) < 0.03)
                interQuartileRange += 0.4;

            double interQuartileRangeMultiplier = interQuartileRange * 1.75;

            double lowerBound = firstQuartileMedian - interQuartileRangeMultiplier;
            double upperBound = thirdQuartileMedian + interQuartileRangeMultiplier;

            return latestReading < lowerBound || latestReading > upperBound;
            //return min < lowerBound || max > upperBound;
        }

        private double GetQuantileMedian(double[] quantile)
        {
            if (quantile == null) throw new ArgumentNullException(nameof(quantile));

            if (quantile.Length % 2 == 0) // even
            {
                int startIndex = (quantile.Length / 2) - 1;
                int endIndex = startIndex + 1;

                return (quantile[startIndex] + quantile[endIndex]) / 2;
            }
            else // odd
            {
                int medianIndex = ((quantile.Length + 1) / 2) - 1;
                return quantile[medianIndex];
            }
        }
    }
}
