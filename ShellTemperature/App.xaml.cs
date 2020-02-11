using BluetoothService.BluetoothServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShellTemperature.Models;
using ShellTemperature.Repository;
using ShellTemperature.ViewModels.ConnectionObserver;
using ShellTemperature.ViewModels.ViewModels;
using ShellTemperature.ViewModels.ViewModels.LadleShell;
using ShellTemperature.Views;
using System;
using System.Linq;
using System.Windows;
using CustomDialog.Interfaces;
using CustomDialog.Services;
using CustomDialog.Views;
using ShellTemperature.ViewModels.TemperatureObserver;

namespace ShellTemperature
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceCollection Services { get; set; }

        private readonly IConfiguration _configuration;

        public App()
        {
            // get the environment variable, for the release version of the app
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") switch
            {
                "Development" => "appsettings.Development.json",
                "Release" => "appsettings.json",
                _ => null
            };

            // add in the app settings file
            var builder = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile(env, optional: false, reloadOnChange: false);

            _configuration = builder.Build();

            ServiceCollection serviceCollection = new ServiceCollection();

            // configure all classes that need to be injected
            ConfigureServices(serviceCollection);
            ConfigureViews(serviceCollection);
            ConfigureViewModels(serviceCollection);
            ConfigDatabase(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            Services = serviceCollection;

            MigrateDatabase();
        }

        /// <summary>
        /// Method initiates the startup of the application.
        /// The MainWindow entity is retrieved from the services provider and rendered to the user
        /// </summary>
        /// <param name="sender">The caller of the method</param>
        /// <param name="e">Any startup arguments provided</param>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            MainWindow mainWindow = _serviceProvider.GetService<MainWindow>();
            mainWindow.Show();
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

        }
        #endregion

        #region Database

        private void ConfigDatabase(IServiceCollection services)
        {
            // database config
            services.AddDbContext<ShellDb>(options =>
                    options.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=ShellDb;Integrated Security=True"
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