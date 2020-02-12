using ShellTemperature.Models;
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
    }
}
