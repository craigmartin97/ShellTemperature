using ShellTemperature.ViewModels.ViewModels;
using System.Windows;
using System.Windows.Media.Animation;

namespace ShellTemperature.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _stateClosed;

        public MainWindow(MainWindowViewModel windowViewModel)
        {
            InitializeComponent();
            DataContext = windowViewModel;

            // init to display nav bar
            NavigationSelectionArea.Visibility = Visibility.Visible;
            GridMenu.Width = 200;
        }

        private void ButtonMenu_OnClick(object sender, RoutedEventArgs e)
        {
            if (_stateClosed)
            {
                ApplyStoryBoard("OpenMenu");


                NavigationSelectionArea.Visibility = Visibility.Visible;
            }
            else
            {

                ApplyStoryBoard("CloseMenu");

                NavigationSelectionArea.Visibility = Visibility.Collapsed;
            }

            _stateClosed = !_stateClosed;
        }

        private void ApplyStoryBoard(string storyboardName)
        {
            Storyboard sb = FindResource(storyboardName) as Storyboard;
            sb?.Begin();
        }
    }
}
