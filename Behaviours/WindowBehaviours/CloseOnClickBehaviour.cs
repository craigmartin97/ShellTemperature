using System.Windows;
using System.Windows.Controls;

namespace Behaviours.WindowBehaviours
{
    /// <summary>
    /// CloseOnClickBehaviour is an attached behaviour which can be attached to buttons
    /// in order to close 
    /// </summary>
    public static class CloseOnClickBehaviour
    {
        /// <summary>
        /// Attached property to buttons to close host window
        /// </summary>
        public static readonly DependencyProperty CloseWindowProperty =
            DependencyProperty.RegisterAttached
            (
                "IsWindowClosed",
                typeof(bool),
                typeof(CloseOnClickBehaviour),
                new PropertyMetadata(false, CloseWindowPropertyChanged)
            );

        public static bool GetCloseWindowProperty(DependencyObject obj)
        {
            return (bool)obj.GetValue(CloseWindowProperty);
        }

        public static void SetCloseWindowProperty(DependencyObject obj, bool value)
        {
            obj.SetValue(CloseWindowProperty, value);
        }


        public static void CloseWindowPropertyChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            if (property is Button button) 
                button.Click += OnClick;
        }

        private static void OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                Window window = Window.GetWindow(btn);
                if (window != null) window.Close();
            }
        }
    }
}
