using Plugin.WinUI.MVVMExpress.Diagnostics;

namespace Plugin.WinUI.MVVMExpress.Threading;

/// <summary>
/// UI-thread contract for constructing pages and applying navigation.
/// ViewModels and hosts should hop through <see cref="IMainThread"/> — not MAUI <c>MainThread</c> statics.
/// </summary>
public static class NavigationThread
{
    /// <summary>Resolves the dispatcher used by navigators and dialogs.</summary>
    public static IMainThread Resolve(IMainThread? injected)
        => injected ?? NotificationMarshaller.Effective ?? ImmediateMainThread.Instance;

    /// <summary>
    /// Throws when a page factory is about to run off the UI thread.
    /// Off-thread <c>new Page()</c> is the login/chat ANR path on Android.
    /// </summary>
    public static void EnsurePageFactoryOnMainThread(IMainThread mainThread)
    {
        ArgumentNullException.ThrowIfNull(mainThread);
        if (!mainThread.IsMainThread)
        {
            throw new InvalidOperationException("Page factory must run on the main thread.");
        }
    }

    /// <summary>Writes a breadcrumb when navigation was requested off-thread.</summary>
    public static void TraceOffThread(IMainThread mainThread, IMvvmExpressDiagnostics? diagnostics, string message)
    {
        ArgumentNullException.ThrowIfNull(mainThread);
        ArgumentException.ThrowIfNullOrEmpty(message);
        if (!mainThread.IsMainThread && diagnostics is { IsEnabled: true })
        {
            diagnostics.Trace("nav", message);
        }
    }
}
