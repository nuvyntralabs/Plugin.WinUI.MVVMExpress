# Plugin.WinUI.MVVMExpress

WinUI 3 host for MVVMExpress. Target: `net10.0-windows10.0.19041.0`. DI, DispatcherQueue, window context, and Loaded/Unloaded lifecycle.

Depends on [Plugin.WinUI.MVVMExpress.Core](https://www.nuget.org/packages/Plugin.WinUI.MVVMExpress.Core).

```csharp
var builder = Host.CreateApplicationBuilder();
builder.Services.UseWinUIMvvmExpress(o => o
    .UseFrameNavigation((nav, _) => nav
        .Map<HomeViewModel, HomePage>("home"))
    .UseDialogs()
    .UseAuth<LoginViewModel>());
```

`UseWinUIMvvmExpress` calls `AddMvvmExpress()`, replaces `IMainThread` with `DispatcherQueueMainThread`, and maps `IWindowContext` to `WinUIWindowContext`. `UseFrameNavigation` / `UseDialogs` live in the Navigation and Dialogs packages.

## Install

```bash
dotnet add package Plugin.WinUI.MVVMExpress.Core
dotnet add package Plugin.WinUI.MVVMExpress
```

Version `1.0.1`. Shared / test code can stay on Core + `AddMvvmExpress()` without this package.

License: MIT. Niladri Padhy / MauiEssentials.
