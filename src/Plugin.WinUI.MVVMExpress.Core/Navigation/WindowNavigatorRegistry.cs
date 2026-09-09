namespace Plugin.WinUI.MVVMExpress.Navigation;

/// <summary>In-memory per-window navigator map.</summary>
public sealed class WindowNavigatorRegistry : IWindowNavigatorRegistry
{
    private readonly Dictionary<string, INavigator> _navigators = new(StringComparer.Ordinal);
    private IWindowContext _current = WindowContext.Default;

    /// <inheritdoc />
    public IWindowContext CurrentWindow
    {
        get => _current;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            ArgumentException.ThrowIfNullOrWhiteSpace(value.WindowId);
            _current = value;
        }
    }

    /// <inheritdoc />
    public void Register(IWindowContext window, INavigator navigator)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentException.ThrowIfNullOrWhiteSpace(window.WindowId);
        ArgumentNullException.ThrowIfNull(navigator);
        _navigators[window.WindowId] = navigator;
    }

    /// <inheritdoc />
    public INavigator GetNavigator(IWindowContext window)
    {
        if (TryGetNavigator(window, out var navigator) && navigator is not null)
        {
            return navigator;
        }

        throw new KeyNotFoundException($"No navigator registered for window '{window.WindowId}'.");
    }

    /// <inheritdoc />
    public INavigator GetCurrent() => GetNavigator(CurrentWindow);

    /// <inheritdoc />
    public bool TryGetNavigator(IWindowContext window, out INavigator? navigator)
    {
        ArgumentNullException.ThrowIfNull(window);
        return _navigators.TryGetValue(window.WindowId, out navigator);
    }
}
