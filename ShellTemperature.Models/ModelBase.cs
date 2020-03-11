using System.ComponentModel;

namespace ShellTemperature.Models
{
    public class ModelBase : INotifyPropertyChanged
    {
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