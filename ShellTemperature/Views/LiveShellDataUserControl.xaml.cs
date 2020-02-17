using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShellTemperature.Views
{
    /// <summary>
    /// Interaction logic for LiveShellDataUserControl.xaml
    /// </summary>
    public partial class LiveShellDataUserControl : UserControl
    {
        public LiveShellDataUserControl()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)dataGrid.Items).CollectionChanged += DataOutputUserControl_CollectionChanged;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
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
