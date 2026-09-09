namespace Plugin.WinUI.MVVMExpress.Navigation;

/// <summary>Stores one <see cref="INavigator"/> per <see cref="IWindowContext"/>.</summary>
public interface IWindowNavigatorRegistry
{
    /// <summary>Window used by <see cref="GetCurrent"/>.</summary>
    IWindowContext CurrentWindow { get; set; }

    /// <summary>Associates <paramref name="navigator"/> with <paramref name="window"/>.</summary>
    void Register(IWindowContext window, INavigator navigator);

    /// <summary>Returns the navigator for <paramref name="window"/>, or throws if none is registered.</summary>
    INavigator GetNavigator(IWindowContext window);

    /// <summary>Returns the navigator for <see cref="CurrentWindow"/>.</summary>
    INavigator GetCurrent();

    /// <summary>Tries to get the navigator for <paramref name="window"/>.</summary>
    bool TryGetNavigator(IWindowContext window, out INavigator? navigator);
}
