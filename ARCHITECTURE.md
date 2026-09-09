# Plugin.WinUI.MVVMExpress Architecture

Independent WinUI 3 MVVM family. **0.1.0-preview.** Aligned with the Plugin.Maui.MVVMExpress 1.3 / Plugin.Wpf.MVVMExpress 1.0 Core contract, but **not** a package reference to those families.

## Principles

1. Core is UI-framework-free (`net10.0`).
2. Host / Navigation / Dialogs target `net10.0-windows10.0.19041.0`.
3. ViewModels depend on `INavigator`, `IDialogs`, `IMainThread`.
4. One navigator per `Window` (`IWindowContext` / `WindowNavigatorRegistry`).
5. No sibling MauiEssentials `PackageReference`.
6. No Shell. Frame + `SectionHostViewModel` cover stack and in-place tabs.

## Packages

- `Plugin.WinUI.MVVMExpress.Core` — ViewModels, commands, state, navigation abstractions
- `Plugin.WinUI.MVVMExpress` — `UseWinUIMvvmExpress`, `DispatcherQueueMainThread`, lifecycle
- `Plugin.WinUI.MVVMExpress.Navigation` — `WinUIFrameNavigator`
- `Plugin.WinUI.MVVMExpress.Dialogs` — `WinUIDialogs`, overlay toasts
- `Plugin.WinUI.MVVMExpress.Validation` / `.Pagination` / `.Testing`
- `Plugin.WinUI.MVVMExpress.Templates` — `dotnet new winui-mvvmexpress`

## Host

```csharp
builder.Services.UseWinUIMvvmExpress(o => o
    .UseFrameNavigation((nav, _) => nav.Map<HomeViewModel, HomePage>("home"))
    .UseDialogs()
    .UseAuth<LoginViewModel>());
```

Put a `Frame` named `NavigationHost` in the window. Toasts must not replace `Window.Content`.
