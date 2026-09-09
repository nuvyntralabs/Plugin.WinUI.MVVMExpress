using Microsoft.UI.Xaml;
using Plugin.WinUI.MVVMExpress.Hosting;

namespace Plugin.WinUI.MVVMExpress.Playground;

public sealed partial class SecondaryWindow : Window
{
    public SecondaryWindow()
    {
        InitializeComponent();
        WinUIWindowContext.For(this);
    }
}
