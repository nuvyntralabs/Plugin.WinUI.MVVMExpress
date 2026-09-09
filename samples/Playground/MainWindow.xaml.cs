using Microsoft.UI.Xaml;
using Plugin.WinUI.MVVMExpress.Hosting;
using Plugin.WinUI.MVVMExpress.Navigation;

namespace Plugin.WinUI.MVVMExpress.Playground;

public sealed partial class MainWindow : Window
{
    public MainWindow(INavigator navigator, IWindowNavigatorRegistry registry)
    {
        InitializeComponent();
        registry.Register(WinUIWindowContext.For(this), navigator);
    }
}
