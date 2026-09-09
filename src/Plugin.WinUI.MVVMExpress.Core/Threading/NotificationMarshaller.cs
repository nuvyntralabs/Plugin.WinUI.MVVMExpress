using Plugin.WinUI.MVVMExpress.Diagnostics;

namespace Plugin.WinUI.MVVMExpress.Threading;

/// <summary>
/// Ambient UI-thread hop for property and command notifications.
/// <see cref="UseScope"/> isolates tests; <c>UseMvvmExpress</c> sets <see cref="Current"/> to <c>MauiMainThread</c>.
/// </summary>
public static class NotificationMarshaller
{
    private static readonly AsyncLocal<Scope?> Override = new();

    /// <summary>Process-wide dispatcher. <see langword="null"/> means raise inline (unit tests / Core hosts).</summary>
    public static IMainThread? Current { get; set; }

    /// <summary>When <see langword="false"/>, notifications always raise inline.</summary>
    public static bool MarshalNotifications { get; set; } = true;

    /// <summary>Optional diagnostics for off-thread hops (Debug <c>EnableDiagnostics</c>).</summary>
    public static IMvvmExpressDiagnostics? Diagnostics { get; set; }

    /// <summary>When <see langword="true"/>, raising while off-thread without a hop throws (test / Debug).</summary>
    public static bool ThrowOnOffThreadRaise { get; set; }

    /// <summary>Effective dispatcher: scoped override, then <see cref="Current"/>.</summary>
    public static IMainThread? Effective => Override.Value?.Thread ?? Current;

    /// <summary>Temporarily overrides <see cref="Current"/> for the current async context.</summary>
    public static IDisposable UseScope(IMainThread? thread, bool? marshal = null, IMvvmExpressDiagnostics? diagnostics = null)
    {
        var previous = Override.Value;
        Override.Value = new Scope(thread, marshal, diagnostics, previous);
        return new Pop(previous);
    }

    /// <summary>Raises <paramref name="action"/> on the UI thread when a dispatcher is present and the caller is off-thread.</summary>
    public static void Raise(Action action, IMainThread? instance = null, bool marshal = true)
    {
        ArgumentNullException.ThrowIfNull(action);
        var scope = Override.Value;
        var shouldMarshal = marshal && (scope?.Marshal ?? MarshalNotifications);
        var main = instance ?? Effective;
        if (!shouldMarshal || main is null || main.IsMainThread)
        {
            if (ThrowOnOffThreadRaise && main is { IsMainThread: false })
            {
                throw new InvalidOperationException("Notification raised off the main thread.");
            }

            action();
            return;
        }

        var diagnostics = scope?.Diagnostics ?? Diagnostics;
        if (diagnostics is { IsEnabled: true })
        {
            diagnostics.Trace("thread", "Hopping notification onto IMainThread.");
        }

        main.BeginInvoke(action);
    }

    private sealed class Scope(IMainThread? thread, bool? marshal, IMvvmExpressDiagnostics? diagnostics, Scope? previous)
    {
        public IMainThread? Thread { get; } = thread;

        public bool? Marshal { get; } = marshal;

        public IMvvmExpressDiagnostics? Diagnostics { get; } = diagnostics;

        public Scope? Previous { get; } = previous;
    }

    private sealed class Pop(Scope? previous) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            Override.Value = previous;
            _disposed = true;
        }
    }
}
