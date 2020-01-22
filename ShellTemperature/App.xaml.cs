using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShellTemperature.Models;
using ShellTemperature.Repository;
using ShellTemperature.ViewModels.BluetoothServices;
using ShellTemperature.ViewModels.ViewModels;
using ShellTemperature.Views;
using System;
using System.IO;
using System.Windows;

namespace ShellTemperature
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceCollection Services { get; set; }

        private IConfiguration Configuration { get; set; }

        public App()
        {
            // get the enviroment directory, for app settings
            string envDirectory = (Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/ShellTemperature");
            if (!Directory.Exists(envDirectory)) // app settings directory doesnt exist
            {
                Directory.CreateDirectory(envDirectory); // app settings directory doesnt exist, create it!
            }

            string env = null;
            // get the enviroment variable, for the release version of the app
            switch (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"))
            {
                case "Development":
                    env = "appsettings.Development.json";
                    break;
                case "Release":
                    env = "appsettings.json";
                    break;
            }

            // add in the app settings file
            var builder = new ConfigurationBuilder()
                .SetBasePath(envDirectory)
                .AddJsonFile(env, optional: false, reloadOnChange: false);

            Configuration = builder.Build();

            ServiceCollection serviceCollection = new ServiceCollection();

            // configure all classes that need to be injected
            ConfigureServices(serviceCollection);
            ConfigureViews(serviceCollection);
            ConfigureViewModels(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();

            Services = serviceCollection;
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
            services.AddSingleton<IReceiverBluetoothService, ReceiverBluetoothService>();
            services.AddScoped<IRepository<Models.ShellTemp>, ShellTemperatureRepository>();

            services.AddDbContext<ShellDb>(options =>
                options.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=ShellDb;Integrated Security=True"
                , optionsBuilder =>
                {
                    optionsBuilder.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
                }),
                ServiceLifetime.Transient);
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
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<LiveShellDataViewModel>();
            services.AddSingleton<ShellHistoryViewModel>();
        }
        #endregion
    }
}