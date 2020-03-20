using CustomDialog.Dialogs;
using CustomDialog.Enums;
using CustomDialog.Services;
using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using ShellTemperature.ViewModels.Commands;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ShellTemperature.ViewModels.ViewModels.Management
{
    public class ManagementViewModel : ViewModelBase
    {
        #region Fields
        /// <summary>
        /// Reading comment repository to manipulate data from the database
        /// </summary>
        private readonly IReadingCommentRepository<ReadingComment> _readingCommentRepository;

        /// <summary>
        /// Positions repository to CRUD positions
        /// </summary>
        private readonly IRepository<Positions> _positionsRepository;
        #endregion

        #region Comment Properties
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

        #region Position Properties

        private ObservableCollection<Positions> _positions;
        /// <summary>
        /// Collection of positions from the data source
        /// </summary>
        public ObservableCollection<Positions> Positions
        {
            get => _positions;
            set
            {
                _positions = value;
                OnPropertyChanged(nameof(Positions));
            }
        }

        private Positions _selectedPosition;
        /// <summary>
        /// The selected position from the collection
        /// </summary>
        public Positions SelectedPosition
        {
            get => _selectedPosition;
            set
            {
                if (value != null)
                    UpdatedPosition = value.Position;

                _selectedPosition = value;
                OnPropertyChanged(nameof(SelectedPosition));
            }
        }

        private string _updatedPosition;
        /// <summary>
        /// The updated position text to change the selected position to
        /// </summary>
        public string UpdatedPosition
        {
            get => _updatedPosition;
            set
            {
                _updatedPosition = value;
                OnPropertyChanged(nameof(UpdatedPosition));
            }
        }
        #endregion

        #region Comment Commands
        /// <summary>
        /// Delete the selected comment
        /// </summary>
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

        /// <summary>
        /// Update the selected comment with the entered text inside the corresponding
        /// text box
        /// </summary>
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

        /// <summary>
        /// Clear the selected comment command
        /// </summary>
        public RelayCommand ClearSelectedCommentCommand => new RelayCommand(delegate { SelectedComment = null; });
        #endregion

        #region Position Commands
        /// <summary>
        /// Delete the selected position
        /// </summary>
        public RelayCommand DeletePositionCommand
        => new RelayCommand(delegate
        {
            if (SelectedPosition == null)
                return;

            DialogService service = new DialogService();
            ConfirmationDialogViewModel confirmation = new ConfirmationDialogViewModel(
                "Delete " + SelectedPosition.Position + "?",
                "You are about to delete " + SelectedPosition.Position + " are you sure?");

            DialogResult result = service.OpenDialogService(confirmation);

            if (result != DialogResult.Yes) // User didnt press yes
                return;

            bool deleted;
            try
            {
                deleted = _positionsRepository.Delete(SelectedPosition.Id);
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine(e.Message);
                AlertDialogViewModel errorAlert = new AlertDialogViewModel("Error", "Could not delete position");
                service.OpenDialogService(errorAlert);
                return;
            }

            string title;
            string message;
            if (deleted)
            {
                title = "Successfully deleted " + SelectedPosition.Position;
                message = "The position has been successfully deleted";

                SetPositions();
                SelectedPosition = null;
            }
            else
            {
                title = "Failed to delete " + SelectedPosition.Position;
                message = "The position could not be deleted try again.";
            }

            AlertDialogViewModel alert = new AlertDialogViewModel(title, message);
            service.OpenDialogService(alert);
        });

        /// <summary>
        /// Update the selected position with the entered text from the corresponding text box
        /// </summary>
        public RelayCommand UpdatePositionCommand
        => new RelayCommand(delegate
        {
            if (SelectedPosition == null || string.IsNullOrWhiteSpace(UpdatedPosition))
                return;

            DialogService service = new DialogService();
            ConfirmationDialogViewModel confirmation = new ConfirmationDialogViewModel(
                "Update " + SelectedPosition.Position + "?",
                "Are you sure you want to change " + SelectedPosition.Position + " to " + UpdatedPosition + "?");

            DialogResult result = service.OpenDialogService(confirmation);

            if (result != DialogResult.Yes) // The user did not press yes
                return;

            Positions position = new Positions
            {
                Id = SelectedPosition.Id,
                Position = UpdatedPosition
            };

            bool updated;
            try
            {
                updated = _positionsRepository.Update(position);
            }
            catch (NullReferenceException e)
            {
                Debug.WriteLine(e.Message);
                AlertDialogViewModel errorAlert = new AlertDialogViewModel("Error", "Could not delete position");
                service.OpenDialogService(errorAlert);
                return;
            }

            string title;
            string message;
            if (updated)
            {
                title = "Successfully updated " + SelectedPosition.Position;
                message = "The position " + SelectedPosition.Position + " has successfully been updated";

                SetPositions();
                SelectedPosition = null;
            }
            else
            {
                title = "Failed to update " + SelectedPosition.Position;
                message = "The position was not updated try again.";
            }

            AlertDialogViewModel alert = new AlertDialogViewModel(title, message);
            service.OpenDialogService(alert);
        });

        /// <summary>
        /// Clear the selected position
        /// </summary>
        public RelayCommand ClearSelectedPositionCommand
        => new RelayCommand(delegate
        {
            SelectedPosition = null;
        });
        #endregion

        #region Constructors
        public ManagementViewModel(IReadingCommentRepository<ReadingComment> readingCommentRepository,
            IRepository<Positions> positionsRepository)
        {
            _readingCommentRepository = readingCommentRepository;
            _positionsRepository = positionsRepository;
            SetComments();
            SetPositions();
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Set comments to to all of the comments from the data source
        /// </summary>
        private void SetComments()
            => Comments = new ObservableCollection<ReadingComment>(_readingCommentRepository.GetAll());

        private void SetPositions()
            => Positions = new ObservableCollection<Positions>(_positionsRepository.GetAll());

        #endregion
    }
}