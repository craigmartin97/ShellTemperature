﻿using BluetoothService.BluetoothServices;
using BluetoothService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using ShellTemperature.Data;
using ShellTemperature.Repository;
using ShellTemperature.Repository.Interfaces;
using ShellTemperature.Service;
using ShellTemperature.ViewModels;
using ShellTemperature.ViewModels.ConnectionObserver;
using ShellTemperature.ViewModels.DataManipulation;
using ShellTemperature.ViewModels.Interfaces;
using ShellTemperature.ViewModels.Outliers;
using ShellTemperature.ViewModels.Statistics;
using ShellTemperature.ViewModels.TemperatureObserver;
using ShellTemperature.ViewModels.ViewModels;
using ShellTemperature.ViewModels.ViewModels.LadleShell;
using ShellTemperature.ViewModels.ViewModels.Management;
using ShellTemperature.ViewModels.ViewModels.Reports;
using ShellTemperature.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Windows;
using ShellTemperature.Service.Development;
using ShellTemperature.Service.Live;
using Polly;

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

        private string _enviromentTag;

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
                    _enviromentTag = e.Args[i + 1];
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
                loggingProvider.SetMinimumLevel(LogLevel.Debug);
                loggingProvider.AddConfiguration(_configuration.GetSection("Logging"));
                loggingProvider.AddNLog(_configuration);
            });

            // configure all classes that need to be injected
            ConfigureServices(serviceCollection);
            ConfigureViews(serviceCollection);
            ConfigureViewModels(serviceCollection);

            
            ConfigDatabase(serviceCollection); // Database

            _serviceProvider = serviceCollection.BuildServiceProvider();

            Services = serviceCollection;

            MigrateDatabase();
            SeedDatabase();

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
                string[] devicesToSearchFor = _configuration.GetSection("DevicesToSearchFor")
                    .GetChildren()
                    .Select(configurationSection => configurationSection.Value)
                    .ToArray();

                IList<BluetoothConfiguration> bluetoothConfigurations = new List<BluetoothConfiguration>();

                IConfigurationSection[] devicePins = _configuration.GetSection("DevicePins")
                    .GetChildren().ToArray();

                foreach (var deviceName in devicesToSearchFor)
                {
                    IConfigurationSection pin = devicePins.FirstOrDefault(x => x.Key.Equals(deviceName));
                    if (pin == null)
                        continue;

                    BluetoothConfiguration config = new BluetoothConfiguration
                    {
                        Name = deviceName,
                        Pin = pin.Value
                    };
                    bluetoothConfigurations.Add(config);
                }

                return new BluetoothFinder(bluetoothConfigurations.ToArray());
            });

            //if (_enviromentTag.Equals("Development"))
            //{
            //    services.AddScoped<IShellTemperatureService<ShellTemp>, ShellTemperatureDevelopmentService>();
            //}
            //else if (_enviromentTag.Equals("Live"))
            //{
                // Inject Http Client for MPI API
                services.AddHttpClient<IShellTemperatureService<ShellTemp>, ShellTemperatureLiveService>
                (service =>
                {
                    service.BaseAddress = new Uri(_configuration["APIAddress"]);
                    service.DefaultRequestHeaders.Accept.Clear();
                    service.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                })
                    .AddTransientHttpErrorPolicy(p =>
                        p.OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                            .WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry))))
                    .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30))); ;
            //}

            services.AddScoped<IRepository<ShellTemp>, ShellTemperatureRepository>();
            services.AddScoped<IShellTemperatureRepository<ShellTemp>, ShellTemperatureRepository>();
            services.AddScoped<IDeviceRepository<DeviceInfo>, DevicesRepository>();
            services.AddScoped<IRepository<ShellTemperatureComment>, ShellTemperatureCommentRepository>();
            services.AddScoped<IReadingCommentRepository<ReadingComment>, ReadingCommentRepository>();
            services.AddScoped<IRepository<Positions>, PositionsRepository>();
            services.AddScoped<IRepository<ShellTemperaturePosition>, ShellTemperaturePositionRepository>();
            services.AddScoped<IShellTemperatureRepository<SdCardShellTemp>, SdCardShellTemperatureRepository>();
            services.AddScoped<IRepository<SdCardShellTemperatureComment>, SdCardShellTemperatureCommentRepository>();

            // add the outlier detector
            services.AddSingleton<OutlierDetector>();

            services.AddSingleton<ShellTemperatureRecordConvertion>();

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

            if (string.IsNullOrWhiteSpace(_enviromentTag))
            {
                services.AddSingleton<BaseLiveShellDataViewModel, LiveBluetoothOnlyShellDataViewModel>();
            }
            else
            {
                if (_enviromentTag.Equals("Development")) // Test mode, only access to test database so no way to check for wifi data
                    services.AddSingleton<BaseLiveShellDataViewModel, LiveBluetoothOnlyShellDataViewModel>();
                else if (_enviromentTag.Equals("Live")) // Live mode, also need to check for wifi data
                    services.AddSingleton<BaseLiveShellDataViewModel, LiveWifiAndBluetoothShellDataViewModel>();
            }

            services.AddScoped<ShellHistoryViewModel>();
            services.AddSingleton<ReportViewModel>();
            services.AddSingleton<ManagementViewModel>();

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

        private void SeedDatabase()
        {
            ShellDb context = _serviceProvider.GetRequiredService<ShellDb>();
            int positionsCount = context.Positions.Count();

            if (positionsCount == 0) // no positions
            {
                Positions[] positions = {
                    new Positions("Top"),
                    new Positions("Bottom"),
                    new Positions("Side")
                };

                context.Positions.AddRange(positions);
                context.SaveChanges();
            }
        }
        #endregion
    }
}