using ShellTemperature.Repository;
using ShellTemperature.ViewModels.BluetoothServices;
using ShellTemperature.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace ShellTemperature.ViewModels.ViewModels
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

        private readonly IRepository<Models.ShellTemperature> _shellRepo;

        /// <summary>
        /// Dispatcher timer to read the incoming bluetooth data from
        /// </summary>
        private readonly DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// Background worker to run tasks on seperate thread.
        /// </summary>
        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker();
        #endregion

        #region Properties
        private ObservableCollection<double> _bluetoothData;
        /// <summary>
        /// A list collection of the live data being sent via bluetooth
        /// </summary>
        public ObservableCollection<double> BluetoothData
        {
            get => _bluetoothData;
            set
            {
                _bluetoothData = value;
                OnPropertyChanged(nameof(BluetoothData));
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Start reading the bluetooth service
        /// </summary>
        public RelayCommand StartCommand
        {
            get => new RelayCommand(param =>
            {
                _receiverBluetoothService.ReadData();
            });
        }
        #endregion

        #region Constructors
        public LiveShellDataViewModel(IReceiverBluetoothService receiverBluetoothService, IRepository<Models.ShellTemperature> repository)
        {
            _receiverBluetoothService = receiverBluetoothService;
            _shellRepo = repository;

            BluetoothData = new ObservableCollection<double>();

            // setup the dispatcher timer
            timer.Tick += Timer_Tick;
            timer.Interval = new System.TimeSpan(0, 0, 1);

            // setup the background worker
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerAsync();
        }
        #endregion

        #region Timer & Background Worker
        /// <summary>
        /// Background worker method that runs on seperate thread.
        /// The background worker do work method starts the timer
        /// to execute and read the data every X seconds.
        /// </summary>
        /// <param name="sender">The caller of this action</param>
        /// <param name="e">Any arguments passed</param>
        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        => timer.Start();

        /// <summary>
        /// Timer_tick executes the start command and then retrieves the bluetooth data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, System.EventArgs e)
        {
            try
            {
                StartCommand.Execute(null); // execute the bluetooth reading service.
                double data = _receiverBluetoothService.GetBluetoothData();
                BluetoothData.Add(data);

                _shellRepo.Create(new Models.ShellTemperature
                {
                    Temperature = data,
                    RecordedDateTime = DateTime.Now
                });
            }
            catch(Exception ex)
            {
                Debug.WriteLine("An expcetion occurred");
                timer.Stop(); // stop trying to read data as an error has occurred.
            }
        }
        #endregion
    }
}
