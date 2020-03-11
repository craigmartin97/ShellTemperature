using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using ShellTemperature.ViewModels.Interfaces;
using ShellTemperature.ViewModels.ViewModels.LadleShell;

namespace ShellTemperature.ViewModels.ViewModels.TemperatureNotifier
{
    public abstract class TemperatureNotifierViewModel : BaseLadleShellDataViewModel, IUpdate
    {
        protected TemperatureNotifierViewModel(IReadingCommentRepository<ReadingComment> readingCommentRepository,
            IRepository<ShellTemperatureComment> commentRepository)
            : base(readingCommentRepository, commentRepository) { }

        public abstract void Update();
    }
}