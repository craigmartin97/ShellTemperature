using ExcelDataWriter.Excel;
using ExcelDataWriter.Interfaces;
using OxyPlot;
using OxyPlot.Axes;
using ShellTemperature.Data;
using ShellTemperature.Models;
using ShellTemperature.Repository.Interfaces;
using ShellTemperature.ViewModels.Commands;
using ShellTemperature.ViewModels.TemperatureObserver;
using ShellTemperature.ViewModels.ViewModels.TemperatureNotifier;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ShellTemperature.ViewModels.ViewModels.LadleShell
{
    /// <summary>
    /// ViewModel for retrieving and displaying previous shell history data to the users
    /// </summary>
    public class ShellHistoryViewModel : TemperatureNotifierViewModel
    {

        #region Fields
        /// <summary>
        /// Device repository to get information about past used devices
        /// </summary>
        private readonly IDeviceRepository<DeviceInfo> _deviceRepository;

        /// <summary>
        /// The temperature subject to get and notify of state changes
        /// </summary>
        private readonly TemperatureSubject _temperatureSubject;

        /// <summary>
        /// Shell Temperature repository position
        /// </summary>
        private readonly IRepository<ShellTemperaturePosition> _shellTemperaturePositionRepository;
        #endregion

        #region Properties
        private ObservableCollection<ShellTemperatureRecord> _bluetoothData =
            new ObservableCollection<ShellTemperatureRecord>();
        /// <summary>
        /// A list collection of the live data being sent via bluetooth
        /// </summary>
        public ObservableCollection<ShellTemperatureRecord> BluetoothData
        {
            get => _bluetoothData;
            set
            {
                _bluetoothData = value;
                OnPropertyChanged(nameof(BluetoothData));
            }
        }

        private ObservableCollection<DataPoint> _dataPoints = new ObservableCollection<DataPoint>();
        /// <summary>
        /// The data points on the oxyplot graph
        /// </summary>
        public ObservableCollection<DataPoint> DataPoints
        {
            get => _dataPoints;
            set
            {
                _dataPoints = value;
                OnPropertyChanged(nameof(DataPoints));
            }
        }

        /// <summary>
        /// The private start date field. By default, (OnLoad), get the last
        /// seven days of shell temperatures
        /// </summary>
        private DateTime _start = DateTime.Now.AddDays(-7);
        /// <summary>
        /// The start date to start searching for data shell records from
        /// </summary>
        public DateTime Start
        {
            get => _start;
            set
            {
                // prevent stack overflow
                if (_start == value)
                    return;

                if (value > End) // the Start date cannot be after the end
                    return;

                _start = value;
                OnPropertyChanged(nameof(Start));

                SetBluetoothData();
                SetDataPoints();
            }
        }

        /// <summary>
        /// The private end date field. By default set to the current date time 
        /// to search between
        /// </summary>
        private DateTime _end = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59).AddMilliseconds(59);
        /// <summary>
        /// The end date to stop search for to get ladle shell temperature records for
        /// </summary>
        public DateTime End
        {
            get => _end;
            set
            {
                // prevent stack overflow
                if (_end == value)
                    return;

                // the end date cannot be before the start date, cannot be in future either
                if (value < Start || value > DateTime.Now.Date)
                    return;

                _end = value;
                OnPropertyChanged(nameof(End));

                SetBluetoothData();
                SetDataPoints();
            }
        }

        private ObservableCollection<DeviceInfo> _devices;
        /// <summary>
        /// Collection of all devices used previously to record data
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

        private DeviceInfo _currentDeviceInfo;
        /// <summary>
        /// The current selected device to live the temperature data for
        /// </summary>
        public DeviceInfo CurrentDeviceInfo
        {
            get => _currentDeviceInfo;
            set
            {
                _currentDeviceInfo = value;

                SetBluetoothData();
                SetDataPoints();

                OnPropertyChanged(nameof(CurrentDeviceInfo));
            }
        }

        public RelayCommand DeleteSelected =>
            new RelayCommand(param =>
            {
                if (param == null)
                    return;

                IEnumerable<ShellTemperatureRecord> selectedItems = ((IList)param).Cast<ShellTemperatureRecord>();

                ShellTemperatureRecord[] shellTemperatureRecords = selectedItems as ShellTemperatureRecord[]
                                                                   ?? selectedItems.ToArray();

                // Get the selected live shell temps
                ShellTemp[] selectedShellTemps = shellTemperatureRecords
                    .Where(sdCardData => !sdCardData.IsFromSdCard)
                    .Select(x
                    => new ShellTemp(x.Id, x.Temperature, x.RecordedDateTime, x.Latitude, x.Longitude, x.Device))
                    .ToArray();

                // Get the selected sd card temps
                SdCardShellTemp[] selectedSdCardShellTemps = shellTemperatureRecords
                    .Where(sdCardData => sdCardData.IsFromSdCard)
                    .Select(x
                        => new SdCardShellTemp(x.Id, x.Temperature, x.RecordedDateTime, x.Latitude, x.Longitude, x.Device))
                    .ToArray();

                if (selectedShellTemps.Length > 0)
                    ShellTemperatureRepository.DeleteRange(selectedShellTemps);
                if (selectedSdCardShellTemps.Length > 0)
                    SdCardShellTemperatureRepository.DeleteRange(selectedSdCardShellTemps);

                SetBluetoothData();
                SetDataPoints();
            });
        #endregion

        #region Commands
        /// <summary>
        /// A command to the shell data between the two specified date ranges
        /// to an excel spreadsheet for further analysis.
        /// </summary>
        public RelayCommand SendToExcelCommand =>
        new RelayCommand(delegate
        {
            ShellTemperatureRecord[] shellTemperatureRecords = GetShellTemperatureRecords();

            if (shellTemperatureRecords == null || !shellTemperatureRecords.Any())
                return;

            // path and sheet info for the excel file
            string path = Path.GetTempPath() + "ShellTemperatures.xlsx";
            const string worksheetName = "ShellTempData";

            IExcelData excelData = new ExcelData(path);
            excelData.CreateExcelWorkSheet(path, worksheetName);
            excelData.OpenExcelFile(path, worksheetName);

            IExcelStyler excelStyle = new ExcelStyler(excelData);

            ExcelWriter excelWriter = new ExcelWriter(excelData, excelStyle);

            // Get the properties for the type
            string[] headers = GetHeaders(shellTemperatureRecords[0]);

            excelWriter.WriteHeaders(headers);
            excelWriter.WriteToExcelFile(shellTemperatureRecords);

            excelWriter.OpenFile(path);
        });
        #endregion

        #region Constructors
        public ShellHistoryViewModel(
             IShellTemperatureRepository<ShellTemp> shellTemperature,
             IShellTemperatureRepository<SdCardShellTemp> sdCardShellTemperatureRepository,
             IDeviceRepository<DeviceInfo> deviceRepository,
             TemperatureSubject subject,
             IRepository<ShellTemperaturePosition> shellTemperaturePositionRepository,
             IReadingCommentRepository<ReadingComment> readingCommentRepository,
             IRepository<ShellTemperatureComment> commentRepository,
             IRepository<SdCardShellTemperatureComment> sdCardCommentRepository)
            : base(readingCommentRepository, commentRepository, shellTemperature, sdCardShellTemperatureRepository,
                sdCardCommentRepository)
        {
            _deviceRepository = deviceRepository;
            _temperatureSubject = subject;
            _shellTemperaturePositionRepository = shellTemperaturePositionRepository;

            _temperatureSubject.Attach(this);

            // get the last seven days of temps and display to the user in the list.
            UpdateDevices();
            SetBluetoothData();
            SetDataPoints();
        }
        #endregion

        #region Set Data
        /// <summary>
        /// Sets the bluetooth data collection.
        /// Retrieves the shell temperatures between the start date and end date.
        /// </summary>
        private void SetBluetoothData()
        {
            BluetoothData.Clear();

            if (CurrentDeviceInfo == null) return;

            ShellTemperatureRecord[] records = GetShellTemperatureRecords();

            if (records == null || !records.Any()) return;

            records = records.OrderBy(time => time.RecordedDateTime).ToArray();

            BluetoothData = new ObservableCollection<ShellTemperatureRecord>(records);
        }

        /// <summary>
        /// Go through all of the bluetooth data and set all the points on the graph accordingly
        /// </summary>
        private void SetDataPoints()
        {
            DataPoints.Clear();
            // have points to plot?
            if (BluetoothData.Count == 0)
            {
                DataPoints.Clear();
                return;
            }

            DataPoints = new ObservableCollection<DataPoint>(); // reset collection
            foreach (ShellTemperatureRecord temp in BluetoothData) // plot points
            {
                DataPoints.Add(new DataPoint(DateTimeAxis.ToDouble(temp.RecordedDateTime), temp.Temperature));
            }
        }

        /// <summary>
        /// Update the devices collection
        /// </summary>
        private void UpdateDevices()
        {
            Devices = new ObservableCollection<DeviceInfo>(_deviceRepository.GetAll());
            if (Devices.Count > 0)
                CurrentDeviceInfo = Devices[0];
        }

        /// <summary>
        /// For a given shell temperature record property
        /// extract the name of the property in a correctly
        /// formatted order
        /// </summary>
        /// <param name="record">Shell temperature record</param>
        /// <returns>Return string array of column headers in formatted order</returns>
        private string[] GetHeaders(ShellTemperatureRecord record)
        {
            // Get the properties for the type
            PropertyInfo[] properties = record.GetType().GetProperties().ToArray();
            string[] headers = new string[properties.Length];

            // Extract the order and name and insert into array at correct position
            foreach (var property in properties)
            {
                Attribute[] t = property.GetCustomAttributes(typeof(DisplayAttribute)).ToArray();
                if (t.Length != 1) continue;

                // Get the order for the property
                int order = ((DisplayAttribute)t[0]).Order;
                headers[order] = property.Name;
            }

            return headers;
        }

        /// <summary>
        /// Get all the live and sd card shell temperatures along with
        /// the comments and positions
        /// </summary>
        /// <returns></returns>
        private ShellTemperatureRecord[] GetShellTemperatureRecords()
        {
            // Get live data and live data comments and positions
            ShellTemp[] tempData = ShellTemperatureRepository.GetShellTemperatureData(Start, End,
                CurrentDeviceInfo.DeviceName, CurrentDeviceInfo.DeviceAddress).ToArray();

            ShellTemperatureComment[] liveDataComments = CommentRepository.GetAll()
                .Where(x => x.ShellTemp.RecordedDateTime >= Start
                            && x.ShellTemp.RecordedDateTime <= End)
                .ToArray();

            ShellTemperaturePosition[] positions = _shellTemperaturePositionRepository.GetAll().ToArray();

            // Get SD Card data and SD card data comments
            SdCardShellTemp[] sdCardShellTemps = SdCardShellTemperatureRepository.GetShellTemperatureData(Start, End,
                CurrentDeviceInfo.DeviceName, CurrentDeviceInfo.DeviceAddress).ToArray();

            SdCardShellTemperatureComment[] sdCardComments = SdCardCommentRepository.GetAll().ToArray();

            // Create new temp list of records
            List<ShellTemperatureRecord> records = new List<ShellTemperatureRecord>();

            // For ever item in ShellTemps, find and match the comment that may have been made
            foreach (ShellTemp shellTemp in tempData)
            {
                ShellTemperatureRecord shellTemperatureRecord =
                    new ShellTemperatureRecord(shellTemp.Id, shellTemp.Temperature, shellTemp.RecordedDateTime,
                        shellTemp.Latitude, shellTemp.Longitude, shellTemp.Device, false); // Not from SD

                // Find the comment for the shell temperature
                ShellTemperatureComment comment =
                    liveDataComments.FirstOrDefault(x => x.ShellTemp.Id == shellTemperatureRecord.Id);

                ShellTemperaturePosition position =
                    positions.FirstOrDefault(x => x.ShellTemp.Id == shellTemperatureRecord.Id);

                if (comment?.Comment != null)
                    shellTemperatureRecord.Comment = comment.Comment.Comment;
                if (position?.Position != null)
                    shellTemperatureRecord.Position = position.Position.Position;

                records.Add(shellTemperatureRecord);
            }

            // Find the sd card data shell temps
            foreach (SdCardShellTemp shellTemp in sdCardShellTemps)
            {
                if (!shellTemp.RecordedDateTime.HasValue) // Doesn't have DateTime, skip
                    continue;

                ShellTemperatureRecord shellTemperatureRecord =
                    new ShellTemperatureRecord(shellTemp.Id, shellTemp.Temperature, (DateTime)shellTemp.RecordedDateTime,
                        shellTemp.Latitude, shellTemp.Longitude, shellTemp.Device, true); // This is from SD

                // Find the comment for the sd card shell temperature
                SdCardShellTemperatureComment temp =
                    sdCardComments.FirstOrDefault(x => x.SdCardShellTemp.Id == shellTemperatureRecord.Id);

                if (temp?.Comment != null)
                    shellTemperatureRecord.Comment = temp.Comment.Comment;

                records.Add(shellTemperatureRecord);
            }

            return records.ToArray();
        }
        #endregion

        /// <summary>
        /// Update the latest temperatures to include the latest readings
        /// from the current device
        /// </summary>
        public override void Update()
        {
            ShellTemperatureRecord latestReading = _temperatureSubject?.GetState();

            if (latestReading == null) return;

            if (latestReading.Device.DeviceAddress.Equals(CurrentDeviceInfo.DeviceAddress)
            && End.Date.Equals(DateTime.Today))
            {
                BluetoothData.Add(latestReading);
                DataPoints.Add(new DataPoint(DateTimeAxis.ToDouble(latestReading.RecordedDateTime),
                    latestReading.Temperature));
            }
        }
    }
}
