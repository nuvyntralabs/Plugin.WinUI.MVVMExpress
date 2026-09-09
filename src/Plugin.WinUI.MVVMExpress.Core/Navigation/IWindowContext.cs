namespace Plugin.WinUI.MVVMExpress.Navigation;

/// <summary>
/// Identifies a window. Navigation, dialogs, and toasts are keyed by window — one window is not the app.
/// </summary>
public interface IWindowContext
{
    /// <summary>Stable id for this window (not a MAUI <c>Window</c> reference).</summary>
    string WindowId { get; }
}

/// <summary>String-id <see cref="IWindowContext"/> for tests and hosts.</summary>
public sealed class WindowContext : IWindowContext, IEquatable<WindowContext>
{
    /// <summary>Default window used when the host has not created a MAUI window.</summary>
    public static WindowContext Default { get; } = new("default");

    /// <summary>Creates a context with <paramref name="windowId"/>.</summary>
    public WindowContext(string windowId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(windowId);
        WindowId = windowId;
    }

    /// <inheritdoc />
    public string WindowId { get; }

    /// <inheritdoc />
    public bool Equals(WindowContext? other) => other is not null && WindowId == other.WindowId;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is WindowContext other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(WindowId);

    /// <inheritdoc />
    public override string ToString() => WindowId;
}
