namespace Plugin.WinUI.MVVMExpress.Diagnostics;

/// <summary>Optional lifecycle / navigation traces. Off by default; never a Release cost unless the app enables it.</summary>
public interface IMvvmExpressDiagnostics
{
    /// <summary>Gets a value indicating whether traces are written.</summary>
    bool IsEnabled { get; }

    /// <summary>Writes a diagnostic line when <see cref="IsEnabled"/> is <see langword="true"/>.</summary>
    void Trace(string area, string message);
}

/// <summary>No-op diagnostics (default).</summary>
public sealed class NullDiagnostics : IMvvmExpressDiagnostics
{
    /// <summary>Shared instance.</summary>
    public static NullDiagnostics Instance { get; } = new();

    /// <inheritdoc />
    public bool IsEnabled => false;

    /// <inheritdoc />
    public void Trace(string area, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(area);
        ArgumentNullException.ThrowIfNull(message);
    }
}

/// <summary>Writes traces through <see cref="Action{T}"/> (tests) or a logger callback.</summary>
public sealed class CallbackDiagnostics : IMvvmExpressDiagnostics
{
    private readonly Action<string, string> _write;

    /// <summary>Creates enabled diagnostics.</summary>
    public CallbackDiagnostics(Action<string, string> write)
    {
        ArgumentNullException.ThrowIfNull(write);
        _write = write;
        IsEnabled = true;
    }

    /// <inheritdoc />
    public bool IsEnabled { get; }

    /// <inheritdoc />
    public void Trace(string area, string message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(area);
        ArgumentNullException.ThrowIfNull(message);
        if (IsEnabled)
        {
            _write(area, message);
        }
    }
}
