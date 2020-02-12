using CustomDialog.Commands;
using CustomDialog.Enums;
using CustomDialog.Interfaces;
using System.Windows.Input;

namespace CustomDialog.Dialogs
{
    public class ConfirmationDialogViewModel : BaseDialogViewModel<DialogResult>
    {
        public ICommand YesCommand { get; }

        public ICommand NoCommand { get; }

        public ConfirmationDialogViewModel(string title, string message) : base(title, message)
        {
            YesCommand = new GenericRelayCommand<IDialogWindow>(Yes);
            NoCommand = new GenericRelayCommand<IDialogWindow>(No);
        }

        /// <summary>
        /// Fires when the yes command is executed.
        /// Closes the window with a Positive response.
        /// </summary>
        /// <param name="window">The window to close</param>
        private void Yes(IDialogWindow window)
            => CloseDialogWithResult(window, DialogResult.Yes);

        /// <summary>
        /// Fires when the NoCommand is executed.
        /// Closes the window with a negative response
        /// </summary>
        /// <param name="window">The window to close</param>
        private void No(IDialogWindow window)
            => CloseDialogWithResult(window, DialogResult.No);

    }
}