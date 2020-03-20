using CustomDialog.Annotations;
using CustomDialog.Dialogs;
using CustomDialog.Enums;
using CustomDialog.Interfaces;
using CustomDialog.Services;
using ShellTemperature.Data;
using ShellTemperature.Models;
using ShellTemperature.Repository.Interfaces;
using ShellTemperature.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ShellTemperature.ViewModels.ViewModels.LadleShell
{
    public abstract class BaseLadleShellDataViewModel : ViewModelBase
    {
        #region Fields
        /// <summary>
        /// Reading comment repository that contains the comments
        /// </summary>
        protected readonly IReadingCommentRepository<ReadingComment> ReadingCommentRepository;

        protected readonly IRepository<ShellTemperatureComment> CommentRepository;

        protected readonly IShellTemperatureRepository<ShellTemp> ShellTemperatureRepository;

        protected readonly IShellTemperatureRepository<SdCardShellTemp> SdCardShellTemperatureRepository;

        protected readonly IRepository<SdCardShellTemperatureComment> SdCardCommentRepository;
        #endregion

        #region Constructors
        protected BaseLadleShellDataViewModel(
            IReadingCommentRepository<ReadingComment> readingCommentRepository,
            IRepository<ShellTemperatureComment> commentRepository,
            IShellTemperatureRepository<ShellTemp> shellTemperatureRepository,
            IShellTemperatureRepository<SdCardShellTemp> sdCardShellTemperatureRepository,
            IRepository<SdCardShellTemperatureComment> sdCardCommentRepository)
        {
            ReadingCommentRepository = readingCommentRepository;
            CommentRepository = commentRepository;
            ShellTemperatureRepository = shellTemperatureRepository;
            SdCardShellTemperatureRepository = sdCardShellTemperatureRepository;
            SdCardCommentRepository = sdCardCommentRepository;
        }
        #endregion

        #region Commands
        public RelayCommand CommentCommand
        => new RelayCommand(param =>
        {
            Debug.WriteLine("Add comment to the reading");

            if (param is ShellTemperatureRecord shellTempRecord)
            {
                IEnumerable<string> existingComments = ReadingCommentRepository.GetAll()
                    .Select(x => x.Comment)
                    .OrderBy(x => x);

                // Show the dialog and ask the user for a comment for the temperature
                IDialogService service = new DialogService();
                CommentDialogViewModel vm =
                    new CommentDialogViewModel("Add comment to data reading", shellTempRecord.Comment, existingComments);

                CommentDialogResult result = service.OpenDialogService(vm);

                if (result == null)
                    return;

                if (result.Delete) // User pressed the delete btn so delete the record
                {
                    DeleteComment(shellTempRecord);
                }
                else // The user made a change and press the ok btn
                {
                    EditComment(result.Comment, shellTempRecord);
                }
            }
        });
        #endregion

        private void EditComment(string res, ShellTemperatureRecord shellTempRecord)
        {
            // Validate the users response
            if (string.IsNullOrWhiteSpace(res))
                return;

            if (res.Equals(shellTempRecord.Comment)) // there the same thing dont do anything else
                return;

            res = res.Trim(); // Format the users response and remove invalid chars

            // Create a new comment if it a new comment
            ReadingComment readingComment = ReadingCommentRepository.GetItem(res);

            if (readingComment == null)
            {
                readingComment = new ReadingComment(res);
                ReadingCommentRepository.Create(readingComment);
            }

            // Is live data
            if (!shellTempRecord.IsFromSdCard)
            {
                ShellTemperatureComment dbShellTemperatureComment = CommentRepository.GetAll()
                    .FirstOrDefault(temperatureComment => temperatureComment.ShellTemp.Id
                        .Equals(shellTempRecord.Id));

                // Doesn't  exist so create a new record
                if (dbShellTemperatureComment == null)
                {
                    // Create a ShellTemp object with the passed data from the view
                    ShellTemp temp = new ShellTemp(shellTempRecord.Id, shellTempRecord.Temperature,
                        shellTempRecord.RecordedDateTime, shellTempRecord.Latitude, shellTempRecord.Longitude, shellTempRecord.Device);

                    // Save the comment against the temperature recording
                    ShellTemperatureComment comment = new ShellTemperatureComment(readingComment, temp);
                    CommentRepository.Create(comment);
                }
                else // Update the existing record
                {
                    dbShellTemperatureComment.Comment = readingComment;
                    CommentRepository.Update(dbShellTemperatureComment);
                }
            }
            // Is from the Sd card
            else
            {
                SdCardShellTemperatureComment dbShellTemperatureComment = SdCardCommentRepository.GetAll()
                    .FirstOrDefault(temperatureComment => temperatureComment.SdCardShellTemp.Id
                        .Equals(shellTempRecord.Id));

                // Doesn't exist, so create a new comment
                if (dbShellTemperatureComment == null)
                {
                    // Create a ShellTemp object with the passed data from the view
                    SdCardShellTemp temp = new SdCardShellTemp(shellTempRecord.Id, shellTempRecord.Temperature,
                        shellTempRecord.RecordedDateTime, shellTempRecord.Latitude, shellTempRecord.Longitude, shellTempRecord.Device);

                    // Save the comment against the temperature recording
                    SdCardShellTemperatureComment comment = new SdCardShellTemperatureComment(readingComment, temp);
                    SdCardCommentRepository.Create(comment);
                }
                else // Update comment
                {
                    dbShellTemperatureComment.Comment = readingComment;
                    SdCardCommentRepository.Update(dbShellTemperatureComment);
                }
            }

            shellTempRecord.Comment = res;
        }

        private void DeleteComment(ShellTemperatureRecord shellTemperatureRecord)
        {
            if (shellTemperatureRecord == null) // invalid obj
                return;
            if (string.IsNullOrWhiteSpace(shellTemperatureRecord.Comment)) // no comment to remove
                return;

            DialogService service = new DialogService();
            ConfirmationDialogViewModel confirmation = new ConfirmationDialogViewModel(
                "Delete " + shellTemperatureRecord.Comment + "?",
                "Are you sure you want to remove the comment " + shellTemperatureRecord.Comment + "?");

            DialogResult result = service.OpenDialogService(confirmation);

            if (result != DialogResult.Yes) // User didnt press yes
                return;

            bool deleted;
            try
            {
                if (shellTemperatureRecord.IsFromSdCard)
                {
                    SdCardShellTemperatureComment dbShellTemperatureComment = SdCardCommentRepository.GetAll()
                        .FirstOrDefault(comment => comment.SdCardShellTemp.Id.Equals(shellTemperatureRecord.Id));

                    if (dbShellTemperatureComment == null)
                        return;

                    deleted = CommentRepository.Delete(dbShellTemperatureComment.Id);
                }
                else
                {
                    ShellTemperatureComment dbShellTemperatureComment = CommentRepository.GetAll()
                        .FirstOrDefault(comment => comment.ShellTemp.Id.Equals(shellTemperatureRecord.Id));

                    if (dbShellTemperatureComment == null)
                        return;

                    deleted = CommentRepository.Delete(dbShellTemperatureComment.Id);
                }

            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e);
                throw;
            }

            string title;
            string message;
            if (deleted)
            {
                title = "Successfully deleted " + shellTemperatureRecord.Comment;
                message = "The comment from the shell temperature has successfully been removed";

                shellTemperatureRecord.Comment = null;
            }
            else
            {
                title = "Failed to delete " + shellTemperatureRecord.Comment;
                message = "The shell temperature record could not be removed";
            }

            AlertDialogViewModel alert = new AlertDialogViewModel(title, message);
            service.OpenDialogService(alert);
        }
    }
}