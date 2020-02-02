using OxyPlot;
using OxyPlot.Axes;
using ShellTemperature.Models;
using ShellTemperature.Repository;
using System;
using System.Collections.ObjectModel;

namespace ShellTemperature.ViewModels.ViewModels.LadleShell
{
    /// <summary>
    /// ViewModel for retrieving and displaying previous shell history data to the users
    /// </summary>
    public class ShellHistoryViewModel : BaseShellViewModel
    {
        #region Fields
        private readonly IShellTemperatureRepository<ShellTemp> _shellTempRepo;
        #endregion

        #region Properties
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
        private DateTime _end = DateTime.Now;
        /// <summary>
        /// The end date to stop search for to get ladle shell temperature records for
        /// </summary>
        public DateTime End
        {
            get => _end;
            set
            {
                if (value < Start) // the end date cannot be before the start date
                    return;

                _end = value;
                OnPropertyChanged(nameof(End));

                SetBluetoothData();
                SetDataPoints();
            }
        }

        private Device _selectedDevice;

        public sealed override Device SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                _selectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }
        #endregion

        #region Commands

        #endregion

        #region Constructors
        public ShellHistoryViewModel(IShellTemperatureRepository<ShellTemp> shellTemperature)
        {
            _shellTempRepo = shellTemperature;

            // get the last seven days of temps and display to the user in the list.
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
            BluetoothData = new ObservableCollection<ShellTemp>(_shellTempRepo.GetShellTemperatureData(Start, End));
        }

        /// <summary>
        /// Go through all of the bluetooth data and set all the points on the graph accordingly
        /// </summary>
        private void SetDataPoints()
        {
            // have points to plot?
            if (BluetoothData.Count > 0)
            {
                DataPoints = new ObservableCollection<DataPoint>(); // reset collection
                foreach (ShellTemp temp in BluetoothData) // plot points
                {
                    DataPoints.Add(new DataPoint(DateTimeAxis.ToDouble(temp.RecordedDateTime), temp.Temperature));
                }
            }
        }
        #endregion
    }
}
