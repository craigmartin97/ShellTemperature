using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using ShellTemperature.ViewModels.Commands;
using ShellTemperature.ViewModels.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using ExcelDataWriter.Excel;
using ExcelDataWriter.Interfaces;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using ShellTemperature.Models;
using ShellTemperature.ViewModels.DataManipulation;

namespace ShellTemperature.ViewModels.ViewModels.Reports
{
    public class ReportViewModel : ViewModelBase
    {
        #region Fields
        /// <summary>
        /// Shell temperature repository to store shell temperature readings
        /// </summary>
        private readonly IShellTemperatureRepository<ShellTemp> _shellRepository;

        /// <summary>
        /// SD card temperature readings
        /// </summary>
        private IShellTemperatureRepository<SdCardShellTemp> _sdCardShellTemperatureRepository;

        /// <summary>
        /// Comments for shell temperatures
        /// </summary>
        private IRepository<ShellTemperatureComment> _commentRepository;

        /// <summary>
        /// SD card comments repository
        /// </summary>
        private IRepository<SdCardShellTemperatureComment> _sdCardCommentRepository;

        private readonly ShellTemperatureRecordConvertion _shellTemperatureRecordConvertion;

        private readonly IBasicStats _stats;

        private readonly IMeasureSpreadStats _measureSpreadStats;

        private ShellTemperatureRecord[] _temps;
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
                LoadingSpinnerVisibility = true;

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (sender, args) =>
                {
                    Thread.Sleep(1000);
                    /*
                     * If the start equals the value then stop
                     * Needed to prevent call stack exception when day is <= 12
                     */
                    if (_start == value)
                        return;

                    if (value > End) // the start cannot be greater than the end
                        return;

                    _start = value.Date;

                    DateChangeUpdate();

                    OnPropertyChanged(nameof(Start));
                };

                worker.RunWorkerCompleted += RunWorkerCompleted;
                worker.RunWorkerAsync();
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
                LoadingSpinnerVisibility = true;

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (sender, args) =>
                {
                    Thread.Sleep(1000);

                    /*
                     * Needed to prevent call stack exception when <= 12
                     */
                    if (_end == value)
                        return;

                    // the end cannot be less than the start, or in the future
                    if (value < Start || value.Date > DateTime.Now.Date)
                        return;

                    _end = value;

                    DateChangeUpdate();

                    OnPropertyChanged(nameof(End));
                };

                worker.RunWorkerCompleted += RunWorkerCompleted;
                worker.RunWorkerAsync();
            }
        }

        private ObservableCollection<DeviceInfo> _devices;
        /// <summary>
        /// Collection of devices to choose from
        /// </summary>
        public ObservableCollection<DeviceInfo> Devices
        {
            get => _devices;
            set
            {
                _devices = value;
                OnPropertyChanged(nameof(Devices));
            }
        }

        private DeviceInfo _selectedDevice;

        public DeviceInfo SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (value == null)
                    return;

                LoadingSpinnerVisibility = true;

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (sender, args) =>
                {
                    Thread.Sleep(1000); // make spinner spin

                    _selectedDevice = value;
                    OnPropertyChanged(nameof(SelectedDevice));

                    _temps = SelectedDevice == null
                        ? GetShellTemperaturesBetweenRange()
                        : GetShellTemperaturesBetweenRange(SelectedDevice);

                    SetNoRecordsFound();

                    if (!_temps.Any())
                        return;

                    double[] temperatureValues = GetShellTemperaturesBetweenRange(_temps);

                    SetMinimum(temperatureValues);
                    SetMaximum(temperatureValues);
                    SetAverage(temperatureValues);
                    SetRange(temperatureValues);
                    SetMedian(temperatureValues);
                    SetMode(temperatureValues);
                    SetInterquartileRange(temperatureValues);
                    SetStandardDeviation(temperatureValues);
                    SetMeanDeviation(temperatureValues);
                };
                worker.RunWorkerCompleted += RunWorkerCompleted;
                worker.RunWorkerAsync();
            }
        }

        private bool _allDevicesIsChecked = true;

        public bool AllDevicesIsChecked
        {
            get => _allDevicesIsChecked;
            set
            {
                LoadingSpinnerVisibility = true;

                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (sender, args) =>
                {
                    Thread.Sleep(1000); // make spinner spin

                    _allDevicesIsChecked = value;
                    OnPropertyChanged(nameof(AllDevicesIsChecked));

                    if (value) // checked
                    {
                        _temps = GetShellTemperaturesBetweenRange();
                    }
                    else
                    {
                        if (SelectedDevice == null) // User has unchecked box and no selected device has been chosen, display no data
                        {
                            SetAllToZero();
                            return;
                        }

                        // There is a selected device
                        _temps = GetShellTemperaturesBetweenRange(SelectedDevice);
                    }

                    SetNoRecordsFound();

                    if (!_temps.Any()) // Nothing to do, set all to zero
                    {
                        SetAllToZero();
                        return;
                    }

                    double[] temperatureValues = GetShellTemperaturesBetweenRange(_temps);

                    SetMinimum(temperatureValues);
                    SetMaximum(temperatureValues);
                    SetAverage(temperatureValues);
                    SetRange(temperatureValues);
                    SetMedian(temperatureValues);
                    SetMode(temperatureValues);
                    SetInterquartileRange(temperatureValues);
                    SetStandardDeviation(temperatureValues);
                    SetMeanDeviation(temperatureValues);
                };
                worker.RunWorkerCompleted += RunWorkerCompleted;
                worker.RunWorkerAsync();
            }
        }

        private bool _noRecordsFoundVisibility;
        /// <summary>
        /// If the label indicating if no records have been found is visible
        /// </summary>
        public bool NoRecordsFoundVisibility
        {
            get => _noRecordsFoundVisibility;
            set
            {
                _noRecordsFoundVisibility = value;
                OnPropertyChanged(nameof(NoRecordsFoundVisibility));
            }
        }
        #endregion

        #region Stats Properties
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

        #region Visibility Properties

        private bool _loadingSpinnerVisibility = false;
        /// <summary>
        /// The visibility of the loading spinner on the screen
        /// Must be changed in seperate thread to be displayed whilst doing operation
        /// </summary>
        public bool LoadingSpinnerVisibility
        {
            get => _loadingSpinnerVisibility;
            set
            {
                _loadingSpinnerVisibility = value;
                OnPropertyChanged(nameof(LoadingSpinnerVisibility));
            }
        }
        #endregion

        #region Commands
        public RelayCommand SendToExcelCommand
        => new RelayCommand(delegate
        {
            if (!_temps.Any())
                return;

            // path and sheet info for the excel file
            string path = Path.GetTempPath() + "RecordShellTemperatures.xlsx";
            const string worksheetName = "ShellTempData";

            IExcelData excelData = new ExcelData(path);
            excelData.CreateExcelWorkSheet(path, worksheetName);
            excelData.OpenExcelFile(path, worksheetName);

            IExcelStyler excelStyle = new ExcelStyler(excelData);

            ExcelWriter excelWriter = new ExcelWriter(excelData, excelStyle);

            // Get the properties for the type
            string[] headers = _temps[0].GetHeaders();

            excelWriter.WriteHeaders(headers);
            excelWriter.WriteToExcelFile(_temps);

            object[][] calculationHeaders = new object[9][]
            {
                new object[] {nameof(Minimum), Minimum },
                new object[]{nameof(Maximum), Maximum },
                new object[]{nameof(Average), Average },
                new object[]{nameof(Median), Median },
                new object[]{nameof(Mode), Mode },
                new object[]{nameof(Range), Range },
                new object[]{nameof(InterquartileRange), InterquartileRange },
                new object[]{nameof(StandardDeviation), StandardDeviation },
                new object[]{nameof(MeanDeviation), MeanDeviation },

            };
            excelWriter.WriteCalculations(calculationHeaders);

            excelWriter.OpenFile(path);
        });
        #endregion

        #region Constructors

        public ReportViewModel(
            IShellTemperatureRepository<ShellTemp> repository,
            IShellTemperatureRepository<SdCardShellTemp> sdCardShellTemperatureRepository,
            IRepository<ShellTemperatureComment> commentRepository,
            IRepository<SdCardShellTemperatureComment> sdCardCommentRepository,
            IBasicStats basicStats,
            IMeasureSpreadStats measureSpreadStats,
            ShellTemperatureRecordConvertion shellTemperatureRecordConvertion)
        {
            _shellRepository = repository;
            _sdCardShellTemperatureRepository = sdCardShellTemperatureRepository;

            _commentRepository = commentRepository;
            _sdCardCommentRepository = sdCardCommentRepository;

            _stats = basicStats;
            _measureSpreadStats = measureSpreadStats;
            _shellTemperatureRecordConvertion = shellTemperatureRecordConvertion;

            _temps = GetShellTemperaturesBetweenRange();
            SetDevices(_temps);
            SetNoRecordsFound();

            double[] temperatureValues = GetShellTemperaturesBetweenRange(_temps);

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

        private ShellTemperatureRecord[] GetShellTemperaturesBetweenRange()
            => _shellTemperatureRecordConvertion.GetShellTemperatureRecords(Start, End);

        private ShellTemperatureRecord[] GetShellTemperaturesBetweenRange(DeviceInfo deviceInfo)
            => _shellTemperatureRecordConvertion.GetShellTemperatureRecords(Start, End, deviceInfo);

        /// <summary>
        /// Get the shell temperatures between the start and end date ranges
        /// </summary>
        /// <returns>Returns a double array with the shell temperatures
        /// between the two date ranges</returns>
        private double[] GetShellTemperaturesBetweenRange(ShellTemperatureRecord[] temps)
            => temps.Select(x => x.Temperature).ToArray();

        private void SetDevices(ShellTemperatureRecord[] temperatures)
        {
            ShellTemperatureRecord[] unique = temperatures.GroupBy(dev => dev.Device)
                .Select(dev => dev.FirstOrDefault())
                .ToArray();

            DeviceInfo[] devices = unique.Select(dev => dev.Device).ToArray();
            if (!devices.Any())
                return;

            Devices = new ObservableCollection<DeviceInfo>(devices);
        }

        private void SetAllToZero()
        {
            Minimum = 0;
            Maximum = 0;
            Average = 0;
            Range = 0;
            Median = 0;
            Mode = 0;
            InterquartileRange = 0;
            StandardDeviation = 0;
            MeanDeviation = 0;
        }

        private void SetNoRecordsFound()
        {
            NoRecordsFoundVisibility = !_temps.Any();
        }

        private void DateChangeUpdate()
        {
            _temps = SelectedDevice == null
                ? GetShellTemperaturesBetweenRange()
                : GetShellTemperaturesBetweenRange(SelectedDevice);

            double[] temperatureValues = GetShellTemperaturesBetweenRange(_temps);
            SetDevices(_temps);
            SetNoRecordsFound();

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

        private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            => LoadingSpinnerVisibility = false;
    }
}