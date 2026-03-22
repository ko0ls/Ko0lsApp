using System;
using System.Windows.Input;

namespace AutoCADTools.Presentation.Utils
{
  public class RelayCommand<T> : ICommand
  {
    private readonly Predicate<T>? _canExecute;
    private readonly Action<T> _execute;

    public RelayCommand(Predicate<T>? canExecute, Action<T> execute)
    {
      _canExecute = canExecute;
      _execute = execute ?? throw new ArgumentNullException(nameof(execute));
    }

    public bool CanExecute(object? parameter)
    {
      if (_canExecute == null)
        return true;

      try {
        return _canExecute((T) parameter!);
      }
      catch {
        return true;
      }
    }

    public void Execute(object? parameter)
    {
      _execute((T) parameter!);
    }

    public event EventHandler? CanExecuteChanged
    {
      add => CommandManager.RequerySuggested += value;
      remove => CommandManager.RequerySuggested -= value;
    }
  }

  public class RelayCommand : System.Windows.Input.ICommand
  {
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
      _execute = execute ?? throw new ArgumentNullException(nameof(execute));
      _canExecute = canExecute;
    }

    public event System.EventHandler? CanExecuteChanged
    {
      add => System.Windows.Input.CommandManager.RequerySuggested += value;
      remove => System.Windows.Input.CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
  }
}