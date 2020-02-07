using System;
using System.Windows.Input;

namespace CustomDialog.Commands
{
    /// <summary>
    /// A command implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class GenericRelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute = null;
        private readonly Func<T, bool> _canExecute = null;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
        public GenericRelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            if (execute != null)
                _execute = execute;
            else throw new ArgumentNullException("execute");

            _canExecute = canExecute ?? (_ => true);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }

    public class GenericRelayCommand : GenericRelayCommand<object>
    {
        public GenericRelayCommand(Action execute)
            : base(_ => execute()) { }

        public GenericRelayCommand(Action execute, Func<bool> canExecute)
            : base(_ => execute(), _ => canExecute()) { }
    }
}