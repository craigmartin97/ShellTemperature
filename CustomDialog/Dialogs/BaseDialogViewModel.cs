using CustomDialog.Interfaces;

namespace CustomDialog.Dialogs
{
    public abstract class BaseDialogViewModel<T>
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public T DialogResult { get; set; }

        #region Constructors

        protected BaseDialogViewModel(string title, string message)
        {
            Title = title;
            Message = message;
        }
        #endregion

        public void CloseDialogWithResult(IDialogWindow dialog, T result)
        {
            DialogResult = result;
            dialog.DialogResult = (result != null);
        }
    }
}