using CustomDialog.Commands;
using CustomDialog.Interfaces;
using ShellTemperature.Data;
using System.Collections.Generic;
using System.Windows.Input;

namespace CustomDialog.Dialogs
{
    public class CommentDialogViewModel : BaseDialogViewModel<string>
    {
        public ICommand OKCommand { get; set; }

        #region Properties
        private IEnumerable<string> _commentItems;
        /// <summary>
        /// Collection of selection options to choose an existing comment from
        /// </summary>
        public IEnumerable<string> CommentItems
        {
            get => _commentItems;
            set
            {
                _commentItems = value;
                OnPropertyChanged(nameof(CommentItems));
            }
        }

        private string _selectedReadingComment;
        /// <summary>
        /// The selected reading comment the user has made
        /// </summary>
        public string SelectedComment
        {
            get => _selectedReadingComment;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;

                Comment = value;
                _selectedReadingComment = value;
                OnPropertyChanged(nameof(SelectedComment));
            }
        }

        private string _comment;
        /// <summary>
        /// The comment the user has made
        /// </summary>
        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }
        #endregion

        public CommentDialogViewModel(string title, string comment, IEnumerable<string> _commentItems) : base(title)
        {
            Comment = comment;
            CommentItems = _commentItems;

            OKCommand = new GenericRelayCommand<IDialogWindow>(OK);
        }

        //Window must be passed in param to close.
        private void OK(IDialogWindow window)
        {
            if (string.IsNullOrEmpty(Comment) && SelectedComment == null) // No comment / selection has been made
                return;

            CloseDialogWithResult(window, Comment);
        }
    }
}