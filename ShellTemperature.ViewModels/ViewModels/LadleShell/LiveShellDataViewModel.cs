using OxyPlot;
using OxyPlot.Axes;
using ShellTemperature.Models;
using ShellTemperature.Repository;
using ShellTemperature.ViewModels.BluetoothServices;
using ShellTemperature.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace ShellTemperature.ViewModels.ViewModels.LadleShell
{
    /// <summary>
    /// Live shell data view model is responsible for retrieving the live
    /// temperature data and displaying the results to the user
    /// </summary>
    public class LiveShellDataViewModel : ViewModelBase
    {
        #region Private fields
        /// <summary>
        /// The bluetooth service to read incoming data from
        /// </summary>
        private readonly IReceiverBluetoothService _receiverBluetoothService;

        /// <summary>
        /// Repository implementation of the ShellTemperature that allows to Create, Read, Update and Delete
        /// </summary>
        private readonly IRepository<ShellTemp> _shellRepo;

        private readonly DispatcherTimer _timer = new DispatcherTimer();
        #endregion

        #region Properties
        private ObservableCollection<ShellTemp> _bluetoothData;
        /// <summary>
        /// A list collection of the live data being sent via bluetooth
        /// </summary>
        public ObservableCollection<ShellTemp> BluetoothData
        {
            get => _bluetoothData;
            set
            {
                _bluetoothData = value;
                OnPropertyChanged(nameof(BluetoothData));
            }
        }

        private ObservableCollection<DataPoint> _dataPoints;
        /// <summary>
        /// The data points on the oxyplot graph
        /// </summary>
        public ObservableCollection<DataPoint> DataPoints
        {
            get => _dataPoints;
            private set
            {
                _dataPoints = value;
                OnPropertyChanged(nameof(DataPoints));
            }
        }

        private bool _isTimerEnabled = false;
        /// <summary>
        /// Boolean value expressing if the start button is enabled or disabled.
        /// Also, this indicates if the timer is runnig or stopped
        /// </summary>
        public bool IsTimerEnabled
        {
            get => _isTimerEnabled;
            private set
            {
                _isTimerEnabled = value;
                OnPropertyChanged(nameof(IsTimerEnabled));
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Start reading the bluetooth service
        /// </summary>
        public RelayCommand StartCommand
        => new RelayCommand(param =>
        {
            StartTimer();
            _receiverBluetoothService.ReadData();
        });

        /// <summary>
        /// Stop command stops the timer from running and stops the execution
        /// of reading data from the bluetooth device.
        /// </summary>
        public RelayCommand StopCommand
        => new RelayCommand(param =>
        {
            _timer.Stop();
            IsTimerEnabled = true; // enable the start button for press
        });

        #endregion

        #region Constructors
        public LiveShellDataViewModel(IReceiverBluetoothService receiverBluetoothService, IRepository<ShellTemp> repository)
        {
            _receiverBluetoothService = receiverBluetoothService;
            _shellRepo = repository;

            // instanziate the bluetoothdata collection
            BluetoothData = new ObservableCollection<ShellTemp>();
            DataPoints = new ObservableCollection<DataPoint>();

            // setup the dispatcher timer
            _timer.Tick += Timer_Tick;
            _timer.Interval = new TimeSpan(0, 0, 1);
            StartTimer();
        }
        #endregion

        #region Timer Ticker
        /// <summary>
        /// Timer_tick executes the start command and then retrieves the bluetooth data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                StartCommand.Execute(null); // execute the bluetooth reading service.

                double data = _receiverBluetoothService.GetBluetoothData();
                ShellTemp shellTemp = new ShellTemp
                {
                    Temperature = data,
                    RecordedDateTime = DateTime.Now
                };

                BluetoothData.Add(shellTemp); // add the shell temperature to the data feed
                DataPoints.Add(new DataPoint(DateTimeAxis.ToDouble(shellTemp.RecordedDateTime), shellTemp.Temperature));

                _shellRepo.Create(shellTemp); // create a new record in the database.
            }
            catch (Exception)
            {
                Debug.WriteLine("An expcetion occurred");
                StopCommand.Execute(null); // stop the timer from running.
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Start the timer running.
        /// If the timer is not enabled, then run the timer
        /// and set the flag indicating its activity to true
        /// </summary>
        private void StartTimer()
        {
            if (!_timer.IsEnabled) // the timer is not enabled, it is not running!
            {
                _timer.Start();
                _timer.IsEnabled = true;
                IsTimerEnabled = false; // the thread is now running, disable the start button
            }
        }
        #endregion
    }
}
