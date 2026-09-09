# Getting started

Scaffold an app:

```bash
dotnet new install Plugin.WinUI.MVVMExpress.Templates
dotnet new winui-mvvmexpress -n MyApp
```

Or add the packages to an existing WinUI 3 project:

```bash
dotnet add package Plugin.WinUI.MVVMExpress.Core
dotnet add package Plugin.WinUI.MVVMExpress
dotnet add package Plugin.WinUI.MVVMExpress.Navigation
dotnet add package Plugin.WinUI.MVVMExpress.Dialogs
```

```csharp
var builder = Host.CreateApplicationBuilder();
builder.Services.AddSingleton<IAuthState, DemoAuthState>();
builder.Services.UseWinUIMvvmExpress(o => o
    .UseFrameNavigation((nav, _) => nav
        .Map<LoginViewModel, LoginPage>("login")
        .Map<HomeViewModel, HomePage>("home"))
    .UseDialogs()
    .UseAuth<LoginViewModel>());
```

Add a `Frame` named `NavigationHost` to the main window. Toasts must not replace `Window.Content`.

Demo credentials in Playground: `demo@mvvmexpress.dev` / `secret`.

After sign-in, `ResetAsync<HomeViewModel>()` replaces the journal so Back cannot return to login.
