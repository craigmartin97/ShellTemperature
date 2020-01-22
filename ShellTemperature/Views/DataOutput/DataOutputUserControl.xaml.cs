using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShellTemperature.Views.DataOutput
{
    /// <summary>
    /// Interaction logic for DataOutputUserControl.xaml
    /// </summary>
    public partial class DataOutputUserControl : UserControl
    {
        public DataOutputUserControl()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)dataGrid.Items).CollectionChanged += DataOutputUserControl_CollectionChanged;
        }

        private void DataOutputUserControl_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (dataGrid.Items.Count > 0)
            {
                if (VisualTreeHelper.GetChild(dataGrid, 0) is Decorator border)
                {
                    if (border.Child is ScrollViewer scroll) scroll.ScrollToEnd();
                }
            }
        }
    }
}
