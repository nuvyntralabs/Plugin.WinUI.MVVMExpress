using System.Windows.Input;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Input;

/// <summary>Synchronous <see cref="ICommand"/> owned by a ViewModel instance.</summary>
public sealed class ModelCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;
    private readonly WeakCanExecuteChanged _canExecuteChanged = new();

    /// <summary>Creates a command.</summary>
    public ModelCommand(Action execute, Func<bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged
    {
        add => _canExecuteChanged.Add(value);
        remove => _canExecuteChanged.Remove(value);
    }

    /// <inheritdoc />
    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        _execute();
    }

    /// <summary>Raises <see cref="CanExecuteChanged"/>.</summary>
    public void NotifyCanExecuteChanged()
        => NotificationMarshaller.Raise(() => _canExecuteChanged.Raise(this, EventArgs.Empty));
}
