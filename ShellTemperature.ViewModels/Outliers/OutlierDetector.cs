using System;
using System.Collections.Generic;
using System.Linq;
using ShellTemperature.ViewModels.Interfaces;

namespace ShellTemperature.ViewModels.Outliers
{
    public class OutlierDetector
    {
        private readonly IMeasureSpreadStats _measureSpreadStats;

        public OutlierDetector(IMeasureSpreadStats measureSpreadStats)
        {
            _measureSpreadStats = measureSpreadStats;
        }

        public bool IsOutlier(IList<double> temps, double latestReading)
        {
            if (temps.Count == 0) return true;
            if (temps.Count <= 4)
            {
                if (latestReading <= 10)
                {
                    temps.Remove(latestReading);
                }

                return latestReading < 5;
            }

            double[] arrTemps = temps.TakeLast(10).ToArray();
            double interQuartileRange = _measureSpreadStats.InterquartileRange(arrTemps,
                out double firstQuartileMedian,
                out double thirdQuartileMedian);

            if (Math.Abs(interQuartileRange) < 0.03)
                interQuartileRange += 0.4;

            double interQuartileRangeMultiplier = interQuartileRange * 1.75;

            double lowerBound = firstQuartileMedian - interQuartileRangeMultiplier;
            double upperBound = thirdQuartileMedian + interQuartileRangeMultiplier;

            return latestReading < lowerBound || latestReading > upperBound;
        }
    }
}
