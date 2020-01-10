using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Behaviours
{
    /// <summary>
    /// MousePosition is used to retrieve the current position of the users mouse from the 
    /// device.
    /// </summary>
    public class MousePosition
    {
        /// <summary>
        /// Gets the mouse position on the screen in reletion to the control
        /// specifed
        /// </summary>
        /// <returns>Returns point where the mouse is on the screen</returns>
        public Point GetMousePosition(Control control, double left, double top)
        => new Point(Mouse.GetPosition(control).X + left, Mouse.GetPosition(control).Y + top);

    }
}
