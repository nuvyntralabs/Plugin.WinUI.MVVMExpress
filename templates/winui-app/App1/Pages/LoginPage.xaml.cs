using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace App1.Pages;

public sealed partial class LoginPage : Page
{
    public LoginPage() => InitializeComponent();

    private void OnSignIn(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
        {
            viewModel.Password = PasswordBox.Password;
        }
    }
}
