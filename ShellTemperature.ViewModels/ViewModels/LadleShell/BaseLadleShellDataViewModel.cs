using CustomDialog.Dialogs;
using CustomDialog.Interfaces;
using CustomDialog.Services;
using ShellTemperature.Data;
using ShellTemperature.Models;
using ShellTemperature.Repository.Interfaces;
using ShellTemperature.ViewModels.Commands;
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
        #endregion

        #region Constructors
        protected BaseLadleShellDataViewModel(IReadingCommentRepository<ReadingComment> readingCommentRepository,
            IRepository<ShellTemperatureComment> commentRepository)
        {
            ReadingCommentRepository = readingCommentRepository;
            CommentRepository = commentRepository;
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

                string res = service.OpenDialogService(vm);

                // Validate the users response
                if (string.IsNullOrWhiteSpace(res))
                    return;

                if (res.Equals(shellTempRecord.Comment)) // there the same thing dont do anything else
                    return;

                res = res.Trim(); // Format the users response and remove invalid chars

                // Create a ShellTemp object with the passed data from the view
                ShellTemp temp = new ShellTemp(shellTempRecord.Id, shellTempRecord.Temperature,
                shellTempRecord.RecordedDateTime, shellTempRecord.Latitude, shellTempRecord.Longitude, shellTempRecord.Device);

                // Create a new comment if it a new comment
                ReadingComment readingComment = ReadingCommentRepository.GetItem(res);

                if (readingComment == null)
                {
                    readingComment = new ReadingComment(res);
                    ReadingCommentRepository.Create(readingComment);
                }

                // Save the comment against the temperature recording
                ShellTemperatureComment comment = new ShellTemperatureComment(readingComment, temp);
                CommentRepository.Create(comment);

                shellTempRecord.Comment = res;
            }
        });
        #endregion

    }
}