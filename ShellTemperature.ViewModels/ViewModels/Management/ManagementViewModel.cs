using CustomDialog.Dialogs;
using CustomDialog.Enums;
using CustomDialog.Services;
using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using ShellTemperature.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;

namespace ShellTemperature.ViewModels.ViewModels.Management
{
    public class ManagementViewModel : ViewModelBase
    {
        #region Fields
        /// <summary>
        /// Reading comment repository to manipulate data from the database
        /// </summary>
        private readonly IReadingCommentRepository<ReadingComment> _readingCommentRepository;

        #endregion
        #region Properties
        private ObservableCollection<ReadingComment> _comments;
        /// <summary>
        /// Collection of comments from the data source that can be changed
        /// </summary>
        public ObservableCollection<ReadingComment> Comments
        {
            get => _comments;
            set
            {
                _comments = value;
                OnPropertyChanged(nameof(Comments));
            }
        }

        private ReadingComment _selectedComment;
        /// <summary>
        /// The selected comment the user has chosen
        /// </summary>
        public ReadingComment SelectedComment
        {
            get => _selectedComment;
            set
            {
                if (value != null)
                    UpdatedComment = value.Comment;

                _selectedComment = value;
                OnPropertyChanged(nameof(SelectedComment));
            }
        }

        private string _updatedComment;
        /// <summary>
        /// The new comment to replace the selected comment with
        /// </summary>
        public string UpdatedComment
        {
            get => _updatedComment;
            set
            {
                _updatedComment = value;
                OnPropertyChanged(nameof(UpdatedComment));
            }
        }
        #endregion

        #region Commands
        public RelayCommand DeleteCommentCommand
        => new RelayCommand(delegate
        {
            if (SelectedComment == null)
                return;

            DialogService service = new DialogService();

            ConfirmationDialogViewModel confirmation = new ConfirmationDialogViewModel(
                "Delete " + SelectedComment.Comment + "?",
                "Are you sure you want to delete this comment? The comment will be removed from all entries");

            DialogResult response = service.OpenDialogService(confirmation);

            if (response != DialogResult.Yes) // The user has NOT pressed yes, so abort!!!
                return;

            bool success;
            try
            {
                success = _readingCommentRepository.Delete(SelectedComment.Id);
            }
            catch (NullReferenceException e)
            {
                service.OpenDialogService(new AlertDialogViewModel("Error", e.Message));
                return; // stop execution
            }

            string title;
            string message;
            if (success)
            {
                title = "Successfully deleted " + SelectedComment.Comment;
                message = "The comment has been removed";

                SelectedComment = null;
                SetComments();
            }
            else
            {
                title = "Failed to delete " + SelectedComment.Comment;
                message = "The comment could not be removed";
            }

            AlertDialogViewModel alert = new AlertDialogViewModel(title, message);
            service.OpenDialogService(alert);
        });

        public RelayCommand UpdateCommentCommand
        => new RelayCommand(delegate
        {
            if (SelectedComment == null || string.IsNullOrWhiteSpace(UpdatedComment))
                return;

            DialogService service = new DialogService();
            ConfirmationDialogViewModel confirmation = new ConfirmationDialogViewModel(
                "Update " + SelectedComment.Comment + "?",
                "Are you sure you want to change " + SelectedComment.Comment + " " +
                "to " + UpdatedComment + "?");

            DialogResult result = service.OpenDialogService(confirmation);

            if (result != DialogResult.Yes)
                return;

            ReadingComment updateComment = new ReadingComment
            {
                Id = SelectedComment.Id,
                Comment = UpdatedComment
            };

            bool updated;
            try
            {
                updated = _readingCommentRepository.Update(updateComment);
            }
            catch (NullReferenceException e)
            {
                AlertDialogViewModel errorAlert = new AlertDialogViewModel("Error updating comment",
                    "Could not update the selected comment " + SelectedComment.Comment);
                service.OpenDialogService(errorAlert);
                return;
            }

            string title;
            string message;
            if (updated)
            {
                title = "Successfully updated comment";
                message = "The comment was successfully changed";

                // Update the comment for the selected comment
                SelectedComment = null;
                SetComments();
            }
            else
            {
                title = "Failed to updated the comment";
                message = "The comment could not be updated, please try again";
            }

            AlertDialogViewModel alert = new AlertDialogViewModel(title, message);
            service.OpenDialogService(alert);
        });

        public RelayCommand ClearSelectedCommentCommand => new RelayCommand(delegate { SelectedComment = null; });
        #endregion

        #region Constructors
        public ManagementViewModel(IReadingCommentRepository<ReadingComment> readingCommentRepository)
        {
            _readingCommentRepository = readingCommentRepository;
            SetComments();
        }
        #endregion

        private void SetComments()
            => Comments = new ObservableCollection<ReadingComment>(_readingCommentRepository.GetAll());
    }
}