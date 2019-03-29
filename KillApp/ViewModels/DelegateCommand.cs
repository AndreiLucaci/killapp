using System;
using System.Windows.Input;

namespace KillApp.ViewModels
{
    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _callback;
        private readonly Action _singleCallback;

        public DelegateCommand(Action callback)
        {
            _singleCallback = callback;
        }

        public DelegateCommand(Action<object> callback)
        {
            _callback = callback;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (_callback != null)
            {
                _callback(parameter);
            }
            else
            {
                _singleCallback();
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
