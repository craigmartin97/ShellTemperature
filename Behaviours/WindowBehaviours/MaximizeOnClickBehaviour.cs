using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Behaviours.WindowBehaviours
{
    /// <summary>
    /// MaximizeOnClickBehaviour is an attached proeprty that can be attached to buttons
    /// to maximize the buttons host window.
    /// </summary>
    public static class MaximizeOnClickBehaviour
    {
        /// <summary>
        /// Attach property to minimuze host window
        /// </summary>
        public static readonly DependencyProperty MaximizeWindowProperty =
            DependencyProperty.RegisterAttached
            (
                "IsWindowMaximized",
                typeof(bool),
                typeof(MaximizeOnClickBehaviour),
                new PropertyMetadata(false, MaximizeWindowPropertyChanged)
            );

        public static bool GetMaximizeWindowProperty(DependencyObject obj)
        {
            return (bool)obj.GetValue(MaximizeWindowProperty);
        }


        public static void SetMaximizeWindowProperty(DependencyObject obj, bool value)
        {
            obj.SetValue(MaximizeWindowProperty, value);
        }


        public static void MaximizeWindowPropertyChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            if (property is Button button) 
                button.Click += OnClick;
        }

        private static void OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                Window window = Window.GetWindow(btn);
                if (window != null)
                    window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
        }
    }
}
