using Microsoft.UI.Xaml.Controls;

namespace Plugin.WinUI.MVVMExpress.Playground.Pages;

public sealed partial class LoginPage : Page
{
    public LoginPage() => InitializeComponent();
    private void OnSignIn(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (DataContext is ViewModels.LoginViewModel vm)
        {
            vm.Password = PasswordBox.Password;
        }
    }

}
