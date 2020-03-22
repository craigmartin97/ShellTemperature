using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using ShellTemperature.ViewModels.Commands;
using ShellTemperature.ViewModels.ViewModels.ConnectionStatus;
using ShellTemperature.ViewModels.ViewModels.LadleShell;
using ShellTemperature.ViewModels.ViewModels.Management;
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

        private readonly IReadingCommentRepository<ReadingComment> _readingCommentRepository;
        private readonly IRepository<Positions> _positionRepository;
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

        public RelayCommand ManagementViewCommand =>
            new RelayCommand(delegate { CurrentView = new ManagementViewModel(_readingCommentRepository, _positionRepository); });
        #endregion

        #region Constructor
        public MainWindowViewModel(TopBarViewModel topBarViewModel, LiveShellDataViewModel liveShellDataViewModel,
            ShellHistoryViewModel shellHistoryViewModel, ReportViewModel reportViewModel,
            IReadingCommentRepository<ReadingComment> readingCommentRepository,
            IRepository<Positions> positionRepository)
        {
            _liveShellDataViewModel = liveShellDataViewModel;
            _shellHistoryViewModel = shellHistoryViewModel;
            _reportViewModel = reportViewModel;

            _readingCommentRepository = readingCommentRepository;
            _positionRepository = positionRepository;


            // Change view to the Live Shell Data View
            LiveShellDataViewCommand.Execute(null);
            // Set the connection status view model
            ConnectionStatusViewModel = topBarViewModel;
        }
        #endregion
    }
}
