using BluetoothService.BluetoothServices;
using CustomDialog.Interfaces;
using CustomDialog.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using ShellTemperature.Models;
using ShellTemperature.Repository;
using ShellTemperature.ViewModels.ConnectionObserver;
using ShellTemperature.ViewModels.TemperatureObserver;
using ShellTemperature.ViewModels.ViewModels;
using ShellTemperature.ViewModels.ViewModels.LadleShell;
using ShellTemperature.ViewModels.ViewModels.Reports;
using ShellTemperature.Views;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using ShellTemperature.ViewModels.Interfaces;
using ShellTemperature.ViewModels.Outliers;
using ShellTemperature.ViewModels.Statistics;
using ShellTemperature.ViewModels.ViewModels.Maps;
using ShellTemperature.ViewModels;

namespace ShellTemperature
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public ServiceCollection Services { get; set; }

        private ClearList clearList = new ClearList();

        private IConfiguration _configuration;

        /// <summary>
        /// Method initiates the startup of the application.
        /// The MainWindow entity is retrieved from the services provider and rendered to the user
        /// </summary>
        /// <param name="sender">The caller of the method</param>
        /// <param name="e">Any startup arguments provided</param>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            // global exception handler for unhandled exceptions
            AppDomain domain = AppDomain.CurrentDomain;
            domain.UnhandledException += DomainOnUnhandledException;

            string env = null;
            for (int i = 0; i < e.Args.Length; i++)
            {
                if (e.Args[i].Equals("-config"))
                {
                    switch (e.Args[i + 1])
                    {
                        case "Development":
                            env = "appsettings.Development.json";
                            break;
                        case "Live":
                            env = "appsettings.json";
                            break;
                        default:
                            env = null;
                            break;
                    }
                }
                else if (e.Args[i].Equals("-clear"))
                {
                    clearList.Clear = true;
                    clearList.ClearValue = int.Parse(e.Args[i + 1]);
                }
            }

            // add in the app settings file
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile(env, optional: false, reloadOnChange: false);

            _configuration = builder.Build();

            ServiceCollection serviceCollection = new ServiceCollection();

            // Add NLog
            serviceCollection.AddLogging(loggingProvider =>
            {
                // configure Logging with NLog
                loggingProvider.ClearProviders();
                loggingProvider.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Debug);
                loggingProvider.AddConfiguration(_configuration.GetSection("Logging"));
                loggingProvider.AddNLog(_configuration);
            });

            // configure all classes that need to be injected
            ConfigureServices(serviceCollection);
            ConfigureViews(serviceCollection);
            ConfigureViewModels(serviceCollection);
            ConfigDatabase(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            Services = serviceCollection;

            MigrateDatabase();

            MainWindow mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
        }

        /// <summary>
        ///  Global exception handler in-case of unhandled exception
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Debug.WriteLine("Global Exception Handler Caught: " + ex.Message);
            MessageBox.Show(ex.Message, "Unhandled exception has occurred");
        }

        #region Configuration
        /// <summary>
        /// Method configures all helper class and other services for dp injection
        /// </summary>
        /// <param name="services">Services collection to add to</param>
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<BluetoothConnectionSubject>();
            services.AddSingleton<TemperatureSubject>();

            services.AddSingleton<IReceiverBluetoothService, ReceiverBluetoothService>();
            services.AddSingleton<IBluetoothFinder, BluetoothFinder>(x =>
            {
                // get a string array of the devices to search for
                var devicesToSearchFor = _configuration.GetSection("DevicesToSearchFor")
                    .GetChildren().Select(dev => dev.Value).ToArray();
                return new BluetoothFinder(devicesToSearchFor);
            });

            services.AddSingleton<IDialogService, DialogService>();

            services.AddScoped<IRepository<ShellTemp>, ShellTemperatureRepository>();
            services.AddScoped<IShellTemperatureRepository<ShellTemp>, ShellTemperatureRepository>();
            services.AddScoped<IDeviceRepository<DeviceInfo>, DevicesRepository>();

            // add the outlier detector
            services.AddSingleton<OutlierDetector>();

            // statistics & sorting
            services.AddSingleton<ISorter, SortingAlgorithm>();
            services.AddSingleton<IBasicStats, BasicStats>();
            services.AddSingleton<IMeasureSpreadStats, MeasureSpreadStats>();

            services.AddSingleton(clearList);
        }

        /// <summary>
        /// Method configures any Views that must be dp injected
        /// </summary>
        /// <param name="services">Services collection to add to</param>
        private void ConfigureViews(IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
        }

        /// <summary>
        /// Method configures any ViewModels that must be dp injected
        /// </summary>
        /// <param name="services">Services collection to add to</param>
        private void ConfigureViewModels(IServiceCollection services)
        {
            services.AddSingleton(_configuration); // add in config

            services.AddSingleton<TopBarViewModel>();
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<LiveShellDataViewModel>();
            services.AddScoped<ShellHistoryViewModel>();
            services.AddSingleton<ReportViewModel>();
            services.AddSingleton<GoogleMapViewModel>();

        }
        #endregion

        #region Database

        private void ConfigDatabase(IServiceCollection services)
        {
            // database config
            services.AddDbContext<ShellDb>(options =>
                    options.UseSqlServer(_configuration.GetConnectionString("ShellConnection")
                        , optionsBuilder =>
                        {
                            optionsBuilder.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
                        }),
                ServiceLifetime.Transient);
        }

        /// <summary>
        /// Ensure the database is fully migrated
        /// </summary>
        private void MigrateDatabase()
        {
            var context = _serviceProvider.GetRequiredService<ShellDb>();
            context.Database.Migrate(); // ensure fully migrated
        }
        #endregion
    }
}