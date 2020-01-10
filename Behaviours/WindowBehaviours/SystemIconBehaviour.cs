using System.Windows;
using System.Windows.Controls;

namespace Behaviours.WindowBehaviours
{
    public class SystemIconBehaviour
    {
        /// <summary>
        /// Attached property to buttons to close host window
        /// </summary>
        public static readonly DependencyProperty SystemIconProperty =
            DependencyProperty.RegisterAttached
            (
                "SystemIcon",
                typeof(bool),
                typeof(SystemIconBehaviour),
                new PropertyMetadata(false, SystemIconPropertyChanged)
            );

        public static bool GetSystemIconProperty(DependencyObject obj)
        {
            return (bool)obj.GetValue(SystemIconProperty);
        }


        public static void SetSystemIconProperty(DependencyObject obj, bool value)
        {
            obj.SetValue(SystemIconProperty, value);
        }


        public static void SystemIconPropertyChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            if (property is Button)
            {
                Button btn = property as Button;
                btn.Click += OnClick;
            }
        }

        private static void OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                Window window = Window.GetWindow(btn);
                if (window != null)
                {
                    Point pointOfLogo = btn.PointToScreen(new Point(0d, 0d));
                    MousePosition mousePosition = new MousePosition();

                    // not the best programming here, but if in center screen adjust to top of logo
                    if (pointOfLogo.X > 600)
                        pointOfLogo.X -= 145;
                    if (pointOfLogo.Y > 300)
                        pointOfLogo.Y -= 50;

                    SystemCommands.ShowSystemMenu(window, mousePosition.GetMousePosition(window, pointOfLogo.X, pointOfLogo.Y));
                }
            }
        }
    }
}
