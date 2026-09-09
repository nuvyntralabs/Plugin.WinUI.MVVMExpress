using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Plugin.WinUI.MVVMExpress.Navigation;

namespace Plugin.WinUI.MVVMExpress.Hosting;

/// <summary>Resolves the current <see cref="Frame"/> and content for a window.</summary>
public static class WinUIVisualTree
{
    public static Frame? CurrentFrame(IWindowContext? window = null)
    {
        var host = WinUIWindowContext.TryGetWindow(window);
        if (host?.Content is not DependencyObject root)
        {
            return null;
        }

        if (root is FrameworkElement namedRoot && namedRoot.FindName("NavigationHost") is Frame named)
        {
            return named;
        }

        return FindFrame(root);
    }

    public static FrameworkElement? CurrentView(IWindowContext? window = null)
    {
        var frame = CurrentFrame(window);
        if (frame?.Content is FrameworkElement fromFrame)
        {
            return fromFrame;
        }

        return WinUIWindowContext.TryGetWindow(window)?.Content as FrameworkElement;
    }

    private static Frame? FindFrame(DependencyObject root)
    {
        if (root is Frame frame)
        {
            return frame;
        }

        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var found = FindFrame(VisualTreeHelper.GetChild(root, i));
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }
}
