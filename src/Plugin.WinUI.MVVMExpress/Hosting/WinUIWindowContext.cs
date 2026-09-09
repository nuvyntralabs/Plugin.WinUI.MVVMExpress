using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Plugin.WinUI.MVVMExpress.Navigation;

namespace Plugin.WinUI.MVVMExpress.Hosting;

/// <summary>Maps a WinUI <see cref="Window"/> to an <see cref="IWindowContext"/>.</summary>
public static class WinUIWindowContext
{
    private static readonly ConditionalWeakTable<Window, WindowContext> Map = [];
    private static readonly List<WeakReference<Window>> Windows = [];
    private static int _next;

    public static Window? MainWindow { get; set; }

    public static IWindowContext For(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        Track(window);
        return Map.GetValue(window, static _ => new WindowContext($"window-{Interlocked.Increment(ref _next)}"));
    }

    public static IWindowContext Current
    {
        get
        {
            var window = MainWindow;
            return window is null ? WindowContext.Default : For(window);
        }
    }

    public static Window? TryGetWindow(IWindowContext? context)
    {
        if (context is null)
        {
            return MainWindow;
        }

        foreach (var weak in Windows.ToArray())
        {
            if (!weak.TryGetTarget(out var window))
            {
                continue;
            }

            if (Map.TryGetValue(window, out var mapped) &&
                string.Equals(mapped.WindowId, context.WindowId, StringComparison.Ordinal))
            {
                return window;
            }
        }

        return MainWindow;
    }

    private static void Track(Window window)
    {
        lock (Windows)
        {
            if (Windows.Any(w => w.TryGetTarget(out var existing) && ReferenceEquals(existing, window)))
            {
                return;
            }

            Windows.Add(new WeakReference<Window>(window));
        }
    }
}
