using Plugin.WinUI.MVVMExpress.Navigation;

namespace Plugin.WinUI.MVVMExpress.Testing;

/// <summary>In-memory <see cref="INavigator"/> for unit tests.</summary>
public sealed class FakeNavigator : InMemoryNavigator
{
    /// <summary>Creates a fake navigator.</summary>
    /// <param name="canLeave">Optional dirty-page guard.</param>
    /// <param name="window">Window this stack belongs to.</param>
    public FakeNavigator(Func<Type, bool>? canLeave = null, IWindowContext? window = null)
        : base(canLeave, window)
    {
    }
}
