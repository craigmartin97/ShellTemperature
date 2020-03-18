using ShellTemperature.ViewModels.Commands;
using ShellTemperature.ViewModels.ViewModels.ConnectionStatus;
using ShellTemperature.ViewModels.ViewModels.LadleShell;
using ShellTemperature.ViewModels.ViewModels.Management;
using ShellTemperature.ViewModels.ViewModels.Maps;
using ShellTemperature.ViewModels.ViewModels.Reports;
using System.Reflection;

namespace ShellTemperature.ViewModels.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Injected Objects
        private readonly LiveShellDataViewModel _liveShellDataViewModel;
        private readonly ShellHistoryViewModel _shellHistoryViewModel;
        private readonly ReportViewModel _reportViewModel;
        private readonly GoogleMapViewModel _googleMapViewModel;
        private readonly ManagementViewModel _managementViewModel;
        #endregion

        #region Public Properties
        private string _applicationVersion = "V" + Assembly.GetEntryAssembly()?.GetName().Version;
        /// <summary>
        /// The version the application is currently running at.
        /// This value comes from the Properties/Package of the ShellTemperature.ViewModels
        /// project
        /// </summary>
        public string ApplicationVersion
        {
            get => _applicationVersion;
            set
            {
                _applicationVersion = value;
                OnPropertyChanged(nameof(ApplicationVersion));
            }
        }

        private ViewModelBase _currentView;
        /// <summary>
        /// The current view that is being displayed to the user
        /// </summary>
        public ViewModelBase CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged(nameof(CurrentView));
            }
        }

        private BluetoothConnectionObserverViewModel _connectioStatusViewModel;

        public BluetoothConnectionObserverViewModel ConnectionStatusViewModel
        {
            get => _connectioStatusViewModel;
            set
            {
                _connectioStatusViewModel = value;
                OnPropertyChanged(nameof(ConnectionStatusViewModel));
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Display the live shell data view to the user
        /// </summary>
        public RelayCommand LiveShellDataViewCommand =>
            new RelayCommand(delegate { CurrentView = _liveShellDataViewModel; });

        /// <summary>
        /// Command to show the Shell history view to the user.
        /// </summary>
        public RelayCommand ShellHistoryViewCommand =>
            new RelayCommand(delegate { CurrentView = _shellHistoryViewModel; });

        public RelayCommand ReportHistoryViewCommand =>
            new RelayCommand(delegate { CurrentView = _reportViewModel; });

        public RelayCommand MapViewCommand =>
            new RelayCommand(delegate { CurrentView = _googleMapViewModel; });

        public RelayCommand ManagementViewCommand =>
            new RelayCommand(delegate { CurrentView = _managementViewModel; });
        #endregion

        #region Constructor
        public MainWindowViewModel(TopBarViewModel topBarViewModel, LiveShellDataViewModel liveShellDataViewModel,
            ShellHistoryViewModel shellHistoryViewModel, ReportViewModel reportViewModel,
            GoogleMapViewModel map, ManagementViewModel managementViewModel)
        {
            _liveShellDataViewModel = liveShellDataViewModel;
            _shellHistoryViewModel = shellHistoryViewModel;
            _reportViewModel = reportViewModel;
            _googleMapViewModel = map;
            _managementViewModel = managementViewModel;

            // Change view to the Live Shell Data View
            LiveShellDataViewCommand.Execute(null);
            // Set the connection status view model
            ConnectionStatusViewModel = topBarViewModel;
        }
        #endregion
    }
}
