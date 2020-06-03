using System;
using System.Windows.Input;

namespace Track.SampleWpfCore
{
    public class Command : ICommand
    {
        private readonly Action _action;
        public Command(Action action) => _action = action;
        public bool CanExecute(object parameter) => true;
        public void Execute(object parameter) => _action();
        public event EventHandler CanExecuteChanged;
    }
}