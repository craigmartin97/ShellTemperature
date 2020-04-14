using BluetoothService.BluetoothServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using ShellTemperature.Service;
using ShellTemperature.Service.Live;
using ShellTemperature.ViewModels.ConnectionObserver;
using ShellTemperature.ViewModels.Outliers;
using ShellTemperature.ViewModels.TemperatureObserver;

namespace ShellTemperature.ViewModels.ViewModels.LadleShell
{
    /// <summary>
    /// Live shell data view model is responsible for retrieving the live
    /// temperature data and displaying the results to the user
    /// </summary>
    public class LiveBluetoothOnlyShellDataViewModel : BaseLiveShellDataViewModel
    {
        #region Constructors

        public LiveBluetoothOnlyShellDataViewModel(IBluetoothFinder bluetoothFinder,
            IShellTemperatureRepository<ShellTemp> shellTemperatureRepository,
            IShellTemperatureRepository<SdCardShellTemp> sdCardShellTemperatureRepository,
            IDeviceRepository<DeviceInfo> deviceRepository,
            IConfiguration configuration,
            BluetoothConnectionSubject subject,
            TemperatureSubject temperatureSubject,
            ILogger<LiveBluetoothOnlyShellDataViewModel> logger,
            OutlierDetector outlierDetector,
            ClearList clear,
            IRepository<ShellTemperatureComment> commentRepository,
            IReadingCommentRepository<ReadingComment> readingCommentRepository,
            IRepository<Positions> positionRepository,
            IRepository<ShellTemperaturePosition> shellTempPositionRepository,
            IRepository<SdCardShellTemperatureComment> sdCardCommentRepository)
            : base(bluetoothFinder, shellTemperatureRepository, sdCardShellTemperatureRepository, deviceRepository,
                configuration, subject,
                temperatureSubject, logger, outlierDetector, clear, commentRepository, readingCommentRepository,
                positionRepository, shellTempPositionRepository, sdCardCommentRepository)
        {

        }
        #endregion
    }
}
