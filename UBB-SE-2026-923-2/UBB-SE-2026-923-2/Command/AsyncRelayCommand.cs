namespace UBB_SE_2026_923_2.Command;

using System;
using System.Threading.Tasks;
using System.Windows.Input;

public class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> execute;
    private readonly Func<bool>? canExecute;
    private bool isRunning;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        this.execute = execute;
        this.canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
        => !this.isRunning && (this.canExecute?.Invoke() ?? true);

    public async void Execute(object? parameter)
    {
        await this.ExecuteAsync();
    }

    public async Task ExecuteAsync()
    {
        if (!this.CanExecute(null))
        {
            return;
        }

        try
        {
            this.isRunning = true;
            this.RaiseCanExecuteChanged();
            await this.execute();
        }
        finally
        {
            this.isRunning = false;
            this.RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged()
        => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}