using ShellTemperature.ViewModels.ViewModels;
using System.Windows;

namespace ShellTemperature.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainWindowViewModel windowViewModel)
        {
            InitializeComponent();
            DataContext = windowViewModel;
        }
    }
}
