using System;
using System.Windows.Input;

namespace ScriptList.Core.Commands
{
    public class SimpleCommand: ICommand
    {
        private Action _execute;
        private Func<bool> _canExecute;

        public SimpleCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute?.Invoke();
        }
    }
}
