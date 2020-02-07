using CustomDialog.Dialogs;

namespace CustomDialog.Interfaces
{
    public interface IDialogService
    {
        T OpenDialogService<T>(BaseDialogViewModel<T> dialogViewModel);
    }
}