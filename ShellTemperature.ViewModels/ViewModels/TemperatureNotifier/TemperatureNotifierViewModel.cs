using ShellTemperature.Data;
using ShellTemperature.Repository.Interfaces;
using ShellTemperature.ViewModels.Interfaces;
using ShellTemperature.ViewModels.ViewModels.LadleShell;

namespace ShellTemperature.ViewModels.ViewModels.TemperatureNotifier
{
    public abstract class TemperatureNotifierViewModel : BaseLadleShellDataViewModel, IUpdate
    {
        protected TemperatureNotifierViewModel(
            IReadingCommentRepository<ReadingComment> readingCommentRepository,
            IRepository<ShellTemperatureComment> commentRepository,
            IShellTemperatureRepository<ShellTemp> shellTemperature,
            IShellTemperatureRepository<SdCardShellTemp> sdCardShellTemperatureRepository,
            IRepository<SdCardShellTemperatureComment> sdCardCommentRepository)
            : base(readingCommentRepository, commentRepository, shellTemperature, sdCardShellTemperatureRepository,
                sdCardCommentRepository)
        { }

        public abstract void Update();
    }
}