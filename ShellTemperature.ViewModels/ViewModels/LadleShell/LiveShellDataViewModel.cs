using BluetoothService.BluetoothServices;
using BluetoothService.cs.BluetoothServices;
using OxyPlot;
using OxyPlot.Axes;
using ShellTemperature.Models;
using ShellTemperature.Repository;
using ShellTemperature.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Threading;

namespace ShellTemperature.ViewModels.ViewModels.LadleShell
{
    /// <summary>
    /// Live shell data view model is responsible for retrieving the live
    /// temperature data and displaying the results to the user
    /// </summary>
    public class LiveShellDataViewModel : BaseShellViewModel
    {
        #region Private fields

        /// <summary>
        /// Repository implementation of the ShellTemperature that allows to Create, Read, Update and Delete.
        /// </summary>
        private readonly IRepository<ShellTemp> _shellRepo;
        #endregion

        #region Properties
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
            SelectedDevice.Timer.Start();
            //_receiverBluetoothService.ReadData();
        });

        /// <summary>
        /// Stop command stops the timer from running and stops the execution
        /// of reading data from the bluetooth device.
        /// </summary>
        public RelayCommand StopCommand
        => new RelayCommand(param =>
        {
            SelectedDevice.Timer.Stop(); // stop the current selected timer.
            IsTimerEnabled = true; // enable the start button for press
        });

        #endregion

        #region Constructors
        public LiveShellDataViewModel(IBluetoothFinder bluetoothFinder, IRepository<ShellTemp> repository)
        {
            _shellRepo = repository;

            List<BluetoothDevice> devices = bluetoothFinder.GetBluetoothDevices();

            foreach (BluetoothDevice device in devices)
            {
                Device dev = new Device()
                {
                    Timer = new DispatcherTimer(),
                    CurrentData = 0,
                    DataPoints = new ObservableCollection<DataPoint>(),
                    Temp = new ObservableCollection<ShellTemp>(),
                    BluetoothService = new ReceiverBluetoothService(),
                    BluetoothDevice = device,
                    IsTimerEnabled = false
                };

                dev.Timer.Tick += (sender, args) => Timer_Tick(sender, args, dev);
                dev.Timer.Interval = new TimeSpan(0, 0, 1);
                dev.Timer.Start();
                dev.Timer.IsEnabled = true;
                dev.IsTimerEnabled = false; // the thread is now running, disable the start button

                Devices.Add(dev);
            }

            SelectedDevice = Devices[0];
        }
        #endregion

        #region Timer Ticker
        /// <summary>
        /// Timer_tick executes the start command and then retrieves the bluetooth data.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e, Device device)
        {
            try
            {
                double? recievedData = device.BluetoothService.ReadData(device.BluetoothDevice);

                if (recievedData != null)
                {
                    device.CurrentData = device.BluetoothService.GetBluetoothData();
                    ShellTemp shellTemp = new ShellTemp
                    {
                        Temperature = device.CurrentData,
                        RecordedDateTime = DateTime.Now
                    };

                    device.Temp.Add(shellTemp);
                    device.DataPoints.Add((new DataPoint(DateTimeAxis.ToDouble(shellTemp.RecordedDateTime), shellTemp.Temperature)));

                    _shellRepo.Create(shellTemp); // create a new record in the database.
                }
            }
            catch (Exception)
            {
                Debug.WriteLine("An expcetion occurred");
            }
        }
        #endregion
    }
}
