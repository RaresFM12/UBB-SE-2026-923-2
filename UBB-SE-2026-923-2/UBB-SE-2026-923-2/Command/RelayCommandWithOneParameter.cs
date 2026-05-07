namespace UBB_SE_2026_923_2.Command
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class RelayCommandWithOneParameter<T> : ICommand
    {
        private readonly Action<T> function;

        public RelayCommandWithOneParameter(Action<T> execute)
        {
            this.function = execute;
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
            this.function((T)parameter);
        }
    }
}
