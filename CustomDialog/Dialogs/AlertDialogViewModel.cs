using CustomDialog.Commands;
using CustomDialog.Enums;
using CustomDialog.Interfaces;
using System.Windows.Input;

namespace CustomDialog.Dialogs
{
    public class AlertDialogViewModel : BaseDialogViewModel<DialogResult>
    {
        public ICommand OKCommand { get; set; }

        public AlertDialogViewModel(string title, string message) : base(title, message)
        {
            OKCommand = new GenericRelayCommand<IDialogWindow>(OK);
        }

        //Window must be passed in param to close.
        private void OK(IDialogWindow window)
        {
            CloseDialogWithResult(window, DialogResult.Undefined);
        }
    }
}