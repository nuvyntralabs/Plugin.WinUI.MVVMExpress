# Plugin.WinUI.MVVMExpress.Navigation

`WinUIFrameNavigator` maps ViewModel types to views on a WinUI `Frame`. `INavigator` and `GuardedNavigator` live in Core.

```csharp
o.UseFrameNavigation((nav, _) => nav
    .Map<LoginViewModel, LoginPage>("login")
    .Map<HomeViewModel, HomePage>("home"));

await navigator.ResetAsync<HomeViewModel>();
```

Put a `Frame` named `NavigationHost` in `MainWindow`. There is no Shell host on WinUI 3.

License: MIT.
