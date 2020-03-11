using System.Windows.Input;
using CustomDialog.Commands;
using CustomDialog.Enums;
using CustomDialog.Interfaces;

namespace CustomDialog.Dialogs
{
    public class CommentDialogViewModel : BaseDialogViewModel<string>
    {
        public ICommand OKCommand { get; set; }

        private string _comment;

        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }

        public CommentDialogViewModel(string title, string comment) : base(title)
        {
            OKCommand = new GenericRelayCommand<IDialogWindow>(OK);
            Comment = comment;
        }

        //Window must be passed in param to close.
        private void OK(IDialogWindow window)
        {
            if (string.IsNullOrEmpty(Comment))
                return;

            CloseDialogWithResult(window, Comment);
        }
    }
}