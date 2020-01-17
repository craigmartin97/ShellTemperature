using Microsoft.Extensions.DependencyInjection;
using ShellTemperature.ViewModels.BluetoothServices;
using ShellTemperature.ViewModels.ViewModels;
using ShellTemperature.Views;
using System;
using System.Windows;

namespace ShellTemperature
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            // configure all classes that need to be injected
            ConfigureServices(serviceCollection);
            ConfigureViews(serviceCollection);
            ConfigureViewModels(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        #region Configuration
        /// <summary>
        /// Method configures all helper class and other services for dp injection
        /// </summary>
        /// <param name="services">Services collection to add to</param>
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IReceiverBluetoothService, ReceiverBluetoothService>();
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
        #endregion
    }
}
