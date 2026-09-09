# Plugin.WinUI.MVVMExpress

Modular MVVM for **WinUI 3** on .NET 10: ViewModels, async commands, Frame navigation, dialogs, validation, pagination.

**Product:** MVVMExpress (WinUI 3 family)
**Package prefix:** `Plugin.WinUI.MVVMExpress`
**Status:** `0.1.0-preview`
**This is not** Plugin.Maui.MVVMExpress, Plugin.Wpf.MVVMExpress, or the other desktop families. Independent port — no PackageReference to those packages.

[![NuGet](https://img.shields.io/nuget/v/Plugin.WinUI.MVVMExpress.Core.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.WinUI.MVVMExpress.Core)

Author: [Niladri Prasad Padhy](https://github.com/NiladriPadhy) · Catalog: [MauiEssentials](https://github.com/nuvyntralabs/MauiEssentials) · License: MIT

## Install

```bash
dotnet add package Plugin.WinUI.MVVMExpress.Core
dotnet add package Plugin.WinUI.MVVMExpress
dotnet add package Plugin.WinUI.MVVMExpress.Navigation
dotnet add package Plugin.WinUI.MVVMExpress.Dialogs
```

```csharp
builder.Services.UseWinUIMvvmExpress(o => o
    .UseFrameNavigation((nav, _) => nav
        .Map<HomeViewModel, HomePage>("home"))
    .UseDialogs()
    .UseAuth<LoginViewModel>());
```

There is no Shell host. Use a `Frame` named `NavigationHost`.

## Templates

```bash
dotnet new install Plugin.WinUI.MVVMExpress.Templates
dotnet new winui-mvvmexpress -n MyApp
dotnet new winui-mvvmexpress-page -n Catalog --namespace MyApp
```

## Packages

| Package | TFM | Role |
| --- | --- | --- |
| `Plugin.WinUI.MVVMExpress.Core` | `net10.0` | ViewModels, commands, state, abstractions |
| `Plugin.WinUI.MVVMExpress` | `net10.0-windows10.0.19041.0` | Host, dispatcher, lifecycle |
| `Plugin.WinUI.MVVMExpress.Navigation` | `net10.0-windows10.0.19041.0` | `WinUIFrameNavigator` |
| `Plugin.WinUI.MVVMExpress.Dialogs` | `net10.0-windows10.0.19041.0` | `WinUIDialogs` + overlay toast |
| `Plugin.WinUI.MVVMExpress.Validation` | `net10.0` | DataAnnotations |
| `Plugin.WinUI.MVVMExpress.Pagination` | `net10.0` | Lists / search |
| `Plugin.WinUI.MVVMExpress.Testing` | `net10.0` | Fakes / leak probe |
| `Plugin.WinUI.MVVMExpress.Templates` | `net10.0` | `dotnet new winui-mvvmexpress` |

Playground: `samples/Playground`. Docs: [getting started](docs/getting-started.md) · [API design](API-DESIGN.md) · [parity](API-PARITY.md).

Usual alternative: [CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm).
