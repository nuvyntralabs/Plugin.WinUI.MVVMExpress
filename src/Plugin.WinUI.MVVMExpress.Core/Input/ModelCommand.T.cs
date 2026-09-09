using System.Windows.Input;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Input;

/// <summary>Synchronous <see cref="ICommand"/> with a typed parameter.</summary>
/// <typeparam name="T">Parameter type.</typeparam>
public sealed class ModelCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Func<T?, bool>? _canExecute;
    private readonly WeakCanExecuteChanged _canExecuteChanged = new();

    /// <summary>Creates a command.</summary>
    public ModelCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
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
    public bool CanExecute(object? parameter)
    {
        if (!TryCast(parameter, out var value))
        {
            return false;
        }

        return _canExecute?.Invoke(value) ?? true;
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        if (!TryCast(parameter, out var value) || !CanExecute(parameter))
        {
            return;
        }

        _execute(value);
    }

    /// <summary>Raises <see cref="CanExecuteChanged"/>.</summary>
    public void NotifyCanExecuteChanged()
        => NotificationMarshaller.Raise(() => _canExecuteChanged.Raise(this, EventArgs.Empty));

    private static bool TryCast(object? parameter, out T? value)
    {
        if (parameter is null)
        {
            value = default;
            return default(T) is null;
        }

        if (parameter is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        return false;
    }
}
