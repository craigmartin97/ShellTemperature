using Moq;
using NUnit.Framework;
using ShellTemperature.ViewModels.ViewModels;

namespace ShellTemperature.Tests.ViewModels
{
    public class MainViewModelTests
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        public MainViewModelTests()
        {
            var liveShellDataVM = new Mock<LiveShellDataViewModel>();

            _mainWindowViewModel = new MainWindowViewModel(liveShellDataVM.Object);
        }

        /// <summary>
        /// Check application version is not null
        /// </summary>
        [Test]
        public void ApplicationVersion_NotNull()
        {
            //Assert
            Assert.IsNotNull(_mainWindowViewModel.ApplicationVersion);
        }

        /// <summary>
        /// Check that the application version is the correct format
        /// </summary>
        [Test]
        public void ApplicationVersion_CorrectFormat()
        {
            //Arrage
            char[] appVersionChars = _mainWindowViewModel.ApplicationVersion.ToCharArray();
            bool isValid = true;

            //Act
            foreach (char c in appVersionChars)
            {
                bool isInt = int.TryParse(c.ToString(), out int i);
                // not an int, not a dot and not a v
                if (!isInt && !(c is '.') && !(c is 'V'))
                {
                    isValid = false;
                    break;
                }
            }

            //Assert
            Assert.IsTrue(isValid);
        }
    }
}
