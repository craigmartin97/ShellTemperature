using InTheHand.Net.Sockets;
using ShellTemperature.ViewModels.BluetoothServices;
using ShellTemperature.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ShellTemperature.ViewModels.ViewModels
{
    /// <summary>
    /// Live shell data view model is responsible for retrieving the live
    /// temperature data and displaying the results to the user
    /// </summary>
    public class LiveShellDataViewModel : ViewModelBase
    {
        #region Private fields
        private IReceiverBluetoothService _receiverBluetoothService;
        #endregion

        #region Properties
        private ObservableCollection<string> _bluetoothData;
        /// <summary>
        /// A list collection of the live data being sent via bluetooth
        /// </summary>
        public ObservableCollection<string> BluetoothData
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
        public RelayCommand StartCommand
        {
            get => new RelayCommand(param =>
            {
                _receiverBluetoothService.Start();
            });
        }
        #endregion

        #region Constructors
        public LiveShellDataViewModel(IReceiverBluetoothService receiverBluetoothService)
        {
            _receiverBluetoothService = receiverBluetoothService;
            _receiverBluetoothService.Start();

            string t = _receiverBluetoothService.Data;
            BluetoothData = new ObservableCollection<string>
            {
                t
            };
        }
        #endregion
    }
}
