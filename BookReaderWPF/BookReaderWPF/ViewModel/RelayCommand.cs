using System.Windows.Input;

namespace BookReader.ViewModel;

internal class RelayCommand : ICommand
{
    private readonly Action? _execute;
    private readonly Func<bool>? _canExecute;

    private readonly Action<object?>? _executeWithParam;
    private readonly Func<object?, bool>? _canExecuteWithParam;


    // Konstruktory
    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }

        _execute = execute;
        _canExecute = canExecute;
    }

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        if (execute == null)
        {
            throw new ArgumentNullException(nameof(execute));
        }

        _executeWithParam = execute;
        _canExecuteWithParam = canExecute;
    }


    // Metody
    public bool CanExecute(object? parameter)
    {
        if (_canExecute != null)
        {
            return _canExecute();
        }

        if (_canExecuteWithParam != null)
        {
            return _canExecuteWithParam(parameter);
        }

        return true;
    }

    public void Execute(object? parameter)
    {
        if (_execute != null)
            _execute();
        else if (_executeWithParam != null)
            _executeWithParam(parameter);
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public void RaiseCanExecuteChanged()
    {
        CommandManager.InvalidateRequerySuggested();
    }
}