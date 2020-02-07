using ShellTemperature.ViewModels.Commands;
using ShellTemperature.ViewModels.ViewModels.ConnectionStatus;
using ShellTemperature.ViewModels.ViewModels.LadleShell;
using System.Reflection;

namespace ShellTemperature.ViewModels.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Injected Objects
        private readonly LiveShellDataViewModel _liveShellDataViewModel;
        private readonly ShellHistoryViewModel _shellHistoryViewModel;
        private readonly TopBarViewModel _topBarViewModel;
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
        public RelayCommand LiveShellDataViewCommand
        {
            get => new RelayCommand(param =>
            {
                CurrentView = _liveShellDataViewModel;
            });
        }

        /// <summary>
        /// Command to show the Shell history view to the user.
        /// </summary>
        public RelayCommand ShellHistoryViewCommand
        {
            get => new RelayCommand(param =>
            {
                CurrentView = _shellHistoryViewModel;
            });
        }
        #endregion

        #region Constructor
        public MainWindowViewModel(TopBarViewModel topBarViewModel, LiveShellDataViewModel liveShellDataViewModel, ShellHistoryViewModel shellHistoryViewModel)
        {
            _topBarViewModel = topBarViewModel;
            _liveShellDataViewModel = liveShellDataViewModel;
            _shellHistoryViewModel = shellHistoryViewModel;
            
            CurrentView = _liveShellDataViewModel;
            ConnectionStatusViewModel = _topBarViewModel;
        }
        #endregion
    }
}
