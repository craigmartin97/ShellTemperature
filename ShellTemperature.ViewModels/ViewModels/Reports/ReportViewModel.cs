using ShellTemperature.Models;
using ShellTemperature.Repository;
using System;
using System.Linq;

namespace ShellTemperature.ViewModels.ViewModels.Reports
{
    public class ReportViewModel : ViewModelBase
    {
        #region Fields

        private readonly IShellTemperatureRepository<ShellTemp> _shellRepository;
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

                _start = value;
                SetMinimum(); // change min value as could have changed
                SetAverage();
                OnPropertyChanged(nameof(Start));
            }
        }

        private DateTime _end = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
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
                if (value < Start || value > DateTime.Now.Date)
                    return;

                _end = value;
                SetMaximum(); // change the max value as could have changed
                SetAverage();
                OnPropertyChanged(nameof(End));
            }
        }

        public double _minimum;
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
        #endregion

        #region Constructors

        public ReportViewModel(IShellTemperatureRepository<ShellTemp> repository)
        {
            _shellRepository = repository;

            SetMinimum();
            SetMaximum();
            SetAverage();
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Find the minimum temperature and set the Minimum property
        /// </summary>
        private void SetMinimum()
        {
            ShellTemp[] shellTemps = _shellRepository.GetShellTemperatureData(Start, End)
                .ToArray();

            if (!shellTemps.Any())
                return;

            double tempMin = shellTemps[0].Temperature; // use the first temperature
            for (int i = 1; i < shellTemps.Length; i++)
            {
                if (shellTemps[i].Temperature < tempMin)
                    tempMin = shellTemps[i].Temperature; // found new smaller number
            }

            Minimum = tempMin;
        }

        /// <summary>
        /// Find the maximum temperature and set the Maximum property
        /// </summary>
        private void SetMaximum()
        {
            ShellTemp[] shellTemps = _shellRepository.GetShellTemperatureData(Start, End)
                .ToArray();

            if (!shellTemps.Any())
                return;

            double tempMax = shellTemps[0].Temperature;
            for (int i = 0; i < shellTemps.Length; i++)
            {
                if (shellTemps[i].Temperature > tempMax)
                    tempMax = shellTemps[i].Temperature; // found new max temperature

            }

            Maximum = tempMax;
        }

        /// <summary>
        /// Sum all the temperatures up and divide by the count to get the
        /// average temperature between the two ranges
        /// </summary>
        private void SetAverage()
        {
            ShellTemp[] shellTemps = _shellRepository.GetShellTemperatureData(Start, End)
                .ToArray();

            if (!shellTemps.Any())
                return;

            double total = shellTemps.Sum(t => t.Temperature);
            double average = total / shellTemps.Count();
            Average = Math.Round(average, 2);
        }
        #endregion
    }
}