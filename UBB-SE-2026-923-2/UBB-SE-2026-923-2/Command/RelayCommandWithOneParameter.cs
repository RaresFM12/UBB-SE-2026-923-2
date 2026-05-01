using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace UBB_SE_2026_923_2.Command
{
    public class RelayCommandWithOneParameter<T> : ICommand
    {
        private Action<T> function;

        public RelayCommandWithOneParameter(Action<T> execute)
        {
            function = execute;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (parameter is T)
            {
                return true;
            }

            return false;
        }

        public void Execute(object parameter)
        {
            function((T)parameter);
        }
    }
}
