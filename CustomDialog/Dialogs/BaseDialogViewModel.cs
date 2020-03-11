using System.ComponentModel;
using CustomDialog.Interfaces;

namespace CustomDialog.Dialogs
{
    public abstract class BaseDialogViewModel<T> : INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public T DialogResult { get; set; }

        #region Constructors

        protected BaseDialogViewModel(string title)
        {
            Title = title;
        }

        protected BaseDialogViewModel(string title, string message) : this(title)
        {
            Message = message;
        }
        #endregion

        public void CloseDialogWithResult(IDialogWindow dialog, T result)
        {
            DialogResult = result;
            dialog.DialogResult = (result != null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #region Notify Property Changed
        /// <summary>
        /// Inform the observers that the property has updated
        /// </summary>
        /// <param name="propertyName">The name of the property that has been updated</param>
        protected void OnPropertyChanged(string propertyName)
            => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
            => PropertyChanged?.Invoke(this, e);
        #endregion
    }
}