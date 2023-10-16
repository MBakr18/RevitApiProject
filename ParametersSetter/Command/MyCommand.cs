using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ParametersSetter.Command
{
    public class MyCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public MyCommand(Action<Object> execute, Predicate<Object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }
}
