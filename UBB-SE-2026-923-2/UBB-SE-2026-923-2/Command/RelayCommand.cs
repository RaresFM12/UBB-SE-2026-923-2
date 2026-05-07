namespace UBB_SE_2026_923_2.Command;

using System;
using System.Windows.Input;

public class RelayCommand : ICommand
{
    private readonly Action execute;
    private readonly Func<bool>? canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => this.canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => this.execute();

    public void RaiseCanExecuteChanged()
        => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}