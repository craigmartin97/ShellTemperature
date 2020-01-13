using System.Collections.ObjectModel;

namespace ShellTemperature.ViewModels.ViewModels
{
    /// <summary>
    /// Live shell data view model is responsible for retrieving the live
    /// temperature data and displaying the results to the user
    /// </summary>
    public class LiveShellDataViewModel : ViewModelBase
    {
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

        #region Constructors
        public LiveShellDataViewModel()
        {
            BluetoothData = new ObservableCollection<string>
            {
                "Hello",
                "World"
            };
        }
        #endregion
    }
}
