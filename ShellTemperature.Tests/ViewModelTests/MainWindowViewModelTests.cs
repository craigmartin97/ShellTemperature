using BluetoothService.BluetoothServices;
using CustomDialog.Interfaces;
using CustomDialog.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ShellTemperature.Models;
using ShellTemperature.Repository;
using ShellTemperature.ViewModels.ConnectionObserver;
using ShellTemperature.ViewModels.Interfaces;
using ShellTemperature.ViewModels.Statistics;
using ShellTemperature.ViewModels.TemperatureObserver;
using ShellTemperature.ViewModels.ViewModels;
using ShellTemperature.ViewModels.ViewModels.LadleShell;
using ShellTemperature.ViewModels.ViewModels.Reports;
using System;
using System.Collections.Generic;
using System.Text;

namespace ShellTemperature.Tests.ViewModelTests
{
    public class MainWindowViewModelTests
    {

        //private readonly MainWindowViewModel mainWindowViewModel;
        //public MainWindowViewModelTests()
        //{
        //    Mock<BluetoothConnectionSubject> blt = new Mock<BluetoothConnectionSubject>();
        //    Mock<TopBarViewModel> topBarVM = new Mock<TopBarViewModel>(blt.Object);

        //    var bluetoothFinder = new Mock<BluetoothFinder>(It.IsAny<string[]>());

        //    var repository = new Mock<ShellTemperatureRepository>(It.IsAny<ShellDb>());
        //    var devRepository = new Mock<DevicesRepository>(It.IsAny<ShellDb>());
            

        //    IConfigurationBuilder configuration = new ConfigurationBuilder();
        //    configuration.AddJsonFile("appsettings.Development.json");
        //    IConfiguration config = configuration.Build();

        //    Mock<TemperatureSubject> tempSubj = new Mock<TemperatureSubject>();
        //    var dialogService = new Mock<DialogService>();
        //    var logger = new Mock<ILogger<LiveShellDataViewModel>>();

        //    var sorter = new Mock<SortingAlgorithm>();
        //    var basicStats = new Mock<BasicStats>(sorter.Object);
        //    var measureSpread = new Mock<MeasureSpreadStats>(sorter.Object, basicStats.Object);
        //    var detector = new Mock<ViewModels.Outliers.OutlierDetector>(measureSpread.Object);

        //    Mock<LiveShellDataViewModel> liveShell = new Mock<LiveShellDataViewModel>(
        //        bluetoothFinder.Object,
        //        repository.Object,
        //        devRepository.Object,
        //        config,
        //        blt.Object,
        //        tempSubj.Object,
        //        dialogService.Object,
        //        logger.Object,
        //        detector.Object);



        //    Mock<ShellHistoryViewModel> shellHist = new Mock<ShellHistoryViewModel>();
        //    Mock<ReportViewModel> reportVM = new Mock<ReportViewModel>();
        //    Mock<GoogleMapViewModel> googleMaps = new Mock<GoogleMapViewModel>();

        //    mainWindowViewModel = new MainWindowViewModel(topBarVM.Object, liveShell.Object,
        //        shellHist.Object, reportVM.Object, googleMaps.Object);
        //}

        //[Test]
        //public void HasApplicationVersion()
        //{
        //    // Assert
        //    Assert.IsNotNull(mainWindowViewModel.ApplicationVersion);
        //}
    }
}
