using CustomDialog.Dialogs;
using CustomDialog.Interfaces;
using CustomDialog.Views;

namespace CustomDialog.Services
{
    public class DialogService : IDialogService
    {
        public T OpenDialogService<T>(BaseDialogViewModel<T> dialogViewModel)
        {
            IDialogWindow window = new DialogWindow();
            window.DataContext = dialogViewModel;
            window.ShowDialog();
            return dialogViewModel.DialogResult;
        }
    }
}