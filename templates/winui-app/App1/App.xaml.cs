using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Plugin.WinUI.MVVMExpress.Auth;
using Plugin.WinUI.MVVMExpress.Dialogs;
using Plugin.WinUI.MVVMExpress.Hosting;
using Plugin.WinUI.MVVMExpress.Navigation;
using App1.Pages;

namespace App1;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = Host.CreateApplicationBuilder();
        builder.Services.AddSingleton<IAuthState, DemoAuthState>();
        builder.Services.AddSingleton<IItemStore, MemoryItemStore>();
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<DetailsViewModel>();
        builder.Services.AddTransient<EditViewModel>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<DetailsPage>();
        builder.Services.AddTransient<EditPage>();
        builder.Services.AddSingleton<MainWindow>();
        builder.Services.UseWinUIMvvmExpress(o => o
            .UseFrameNavigation((nav, _) => nav
                .Map<LoginViewModel, LoginPage>("login")
                .Map<HomeViewModel, HomePage>("home")
                .Map<DetailsViewModel, DetailsPage>("details")
                .Map<EditViewModel, EditPage>("edit"))
            .UseDialogs()
            .UseAuth<LoginViewModel>());

        _host = builder.Build();
        await _host.StartAsync().ConfigureAwait(true);

        var window = _host.Services.GetRequiredService<MainWindow>();
        WinUIWindowContext.MainWindow = window;
        window.Activate();

        var navigator = _host.Services.GetRequiredService<INavigator>();
        await navigator.ResetAsync<LoginViewModel>().ConfigureAwait(true);
    }
}
