using ShellTemperature.Models;
using ShellTemperature.Repository;
using ShellTemperature.ViewModels.Interfaces;
using System;
using System.Linq;

namespace ShellTemperature.ViewModels.ViewModels.Reports
{
    public class ReportViewModel : ViewModelBase
    {
        #region Fields

        private readonly IShellTemperatureRepository<ShellTemp> _shellRepository;

        private readonly IBasicStats _stats;

        private readonly IMeasureSpreadStats _measureSpreadStats;
        #endregion

        #region Properties

        private DateTime _start = DateTime.Now.Date;
        /// <summary>
        /// Start datetime of the report selection
        /// Default to the start of today
        /// </summary>
        public DateTime Start
        {
            get => _start;
            set
            {
                /*
                 * If the start equals the value then stop
                 * Needed to prevent call stack exception when day is <= 12
                 */
                if (_start == value)
                    return;

                if (value > End) // the start cannot be greater than the end
                    return;

                _start = value.Date;

                double[] temperatureValues = GetShellTemperaturesBetweenRange();

                SetMinimum(temperatureValues);
                SetMaximum(temperatureValues);
                SetAverage(temperatureValues);
                SetRange(temperatureValues);
                SetMedian(temperatureValues);
                SetMode(temperatureValues);
                SetInterquartileRange(temperatureValues);
                SetStandardDeviation(temperatureValues);
                SetMeanDeviation(temperatureValues);
                OnPropertyChanged(nameof(Start));
            }
        }

        private DateTime _end = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
        /// <summary>
        /// End datetime of the report selection
        /// Default to today end datetime
        /// </summary>
        public DateTime End
        {
            get => _end;
            set
            {
                /*
                 * Needed to prevent call stack exception when <= 12
                 */
                if (_end == value)
                    return;

                // the end cannot be less than the start, or in the future
                if (value < Start || value.Date > DateTime.Now.Date)
                    return;

                _end = value;

                double[] temperatureValues = GetShellTemperaturesBetweenRange();

                SetMinimum(temperatureValues);
                SetMaximum(temperatureValues);
                SetAverage(temperatureValues);
                SetRange(temperatureValues);
                SetMedian(temperatureValues);
                SetMode(temperatureValues);
                SetInterquartileRange(temperatureValues);
                SetStandardDeviation(temperatureValues);
                SetMeanDeviation(temperatureValues);
                OnPropertyChanged(nameof(End));
            }
        }

        private double _minimum;
        /// <summary>
        /// Minimum value for the report.
        /// The minimum is the smallest value found between the two date ranges
        /// </summary>
        public double Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;
                OnPropertyChanged(nameof(Minimum));
            }
        }

        private double _maximum;
        /// <summary>
        /// The maximum temperature recorded between the two date ranges
        /// </summary>
        public double Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;
                OnPropertyChanged(nameof(Maximum));
            }
        }

        private double _average;
        /// <summary>
        /// The average of the found data between the two ranges
        /// </summary>
        public double Average
        {
            get => _average;
            set
            {
                _average = value;
                OnPropertyChanged(nameof(Average));
            }
        }

        private double _median;
        /// <summary>
        /// The median for the report
        /// </summary>
        public double Median
        {
            get => _median;
            set
            {
                _median = value;
                OnPropertyChanged(nameof(Median));
            }
        }

        private double _mode;
        /// <summary>
        /// The mode for the report
        /// </summary>
        public double Mode
        {
            get => _mode;
            set
            {
                _mode = value;
                OnPropertyChanged(nameof(Mode));
            }
        }

        private double _range;
        /// <summary>
        /// The range of the collection of data
        /// </summary>
        public double Range
        {
            get => _range;
            set
            {
                _range = value;
                OnPropertyChanged(nameof(Range));
            }
        }

        private double _interquartileRange;
        /// <summary>
        /// The interquartile range of the data set
        /// </summary>
        public double InterquartileRange
        {
            get => _interquartileRange;
            set
            {
                _interquartileRange = value;
                OnPropertyChanged(nameof(InterquartileRange));
            }
        }

        private double _standardDeviation;
        /// <summary>
        /// The standard deviation for the report
        /// </summary>
        public double StandardDeviation
        {
            get => _standardDeviation;
            set
            {
                _standardDeviation = value;
                OnPropertyChanged(nameof(StandardDeviation));
            }
        }

        private double _meanDeviation;
        /// <summary>
        /// The mean deviation for the report
        /// </summary>
        public double MeanDeviation
        {
            get => _meanDeviation;
            set
            {
                _meanDeviation = value;
                OnPropertyChanged(nameof(MeanDeviation));
            }
        }

        #endregion

        #region Constructors

        public ReportViewModel(IShellTemperatureRepository<ShellTemp> repository
        , IBasicStats basicStats, IMeasureSpreadStats measureSpreadStats)
        {
            _shellRepository = repository;
            _stats = basicStats;
            _measureSpreadStats = measureSpreadStats;

            double[] temperatureValues = GetShellTemperaturesBetweenRange();

            SetMinimum(temperatureValues);
            SetMaximum(temperatureValues);
            SetAverage(temperatureValues);
            SetRange(temperatureValues);
            SetMedian(temperatureValues);
            SetMode(temperatureValues);
            SetInterquartileRange(temperatureValues);
            SetStandardDeviation(temperatureValues);
            SetMeanDeviation(temperatureValues);
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Find the minimum temperature and set the Minimum property
        /// </summary>
        private void SetMinimum(double[] temps)
        {
            if (!temps.Any())
                return;

            Minimum = _stats.Minimum(temps);
        }

        /// <summary>
        /// Find the maximum temperature and set the Maximum property
        /// </summary>
        private void SetMaximum(double[] temps)
        {
            if (!temps.Any())
                return;

            Maximum = _stats.Maximum(temps);
        }

        /// <summary>
        /// Sum all the temperatures up and divide by the count to get the
        /// average temperature between the two ranges
        /// </summary>
        private void SetAverage(double[] temps)
        {
            if (!temps.Any())
                return;

            Average = _stats.Mean(temps);
        }

        private void SetMedian(double[] temps)
        {
            if (!temps.Any())
                return;

            Median = _stats.Median(temps);
        }

        private void SetMode(double[] temps)
        {
            if (!temps.Any())
                return;

            Mode = _stats.Mode(temps);
        }

        /// <summary>
        /// Set the range property
        /// </summary>
        private void SetRange(double[] temps)
        {
            if (!temps.Any())
                return;

            Range = _measureSpreadStats.Range(temps);
        }

        /// <summary>
        /// Calculate the inter-quartile range and assign to the inter-quartile range property
        /// </summary>
        /// <param name="temps">The values to use to calculate the inter-quartile range</param>
        private void SetInterquartileRange(double[] temps)
        {
            if (!temps.Any())
                return;

            InterquartileRange = _measureSpreadStats.InterquartileRange(temps);
        }

        /// <summary>
        /// Calculate the standard deviation and assign to the standard deviation property
        /// </summary>
        /// <param name="temps">The values to calculate the standard deviation for</param>
        private void SetStandardDeviation(double[] temps)
        {
            if (!temps.Any())
                return;

            StandardDeviation = _measureSpreadStats.StandardDeviation(temps);
        }

        /// <summary>
        /// Calculate the mean deviation and assign to the mean deviation property
        /// </summary>
        /// <param name="temps">The values to use to calculate the mean deviation property with</param>
        private void SetMeanDeviation(double[] temps)
        {
            if (!temps.Any())
                return;

            MeanDeviation = _measureSpreadStats.MeanDeviation(temps);
        }

        /// <summary>
        /// Get the shell temperatures between the start and end date ranges
        /// </summary>
        /// <returns>Returns a double array with the shell temperatures
        /// between the two date ranges</returns>
        private double[] GetShellTemperaturesBetweenRange()
        => _shellRepository.GetShellTemperatureData(Start, End)
                .Select(x => x.Temperature)
                .ToArray();
        #endregion
    }
}