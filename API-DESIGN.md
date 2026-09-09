# Plugin.WinUI.MVVMExpress Public API Design

**1.0.0** contract. Namespaces start with `Plugin.WinUI.MVVMExpress`. Core shapes match Maui 1.3 / WPF 1.0 so a ViewModel ports with a namespace swap. This is **not** a type-forward of sibling families.

## Hosting

```csharp
services.AddMvvmExpress();
services.AddWinUIMvvmExpress(configure);
builder.UseWinUIMvvmExpress(configure);
options.UseFrameNavigation(configure);
options.UseDialogs();
options.UseAuth<TChallenge>();
```

## Core (shipped)

`ObservableModel`, `ViewModel`, `PageViewModel`, `ModelCommand`, `AsyncModelCommand`, `AsyncState<T>`, `Outcome`, `IMessageHub`, `INavigator`, `IPageNavigator`, `InMemoryNavigator`, `GuardedNavigator`, `IDialogs`, `INotifier`, `IMainThread`, `IWindowContext`, `FormViewModel`, `SectionHostViewModel`, `IAuthState`, `ICache`, `IConnectivityProbe`.

## Hosts (shipped)

- `DispatcherQueueMainThread`
- `WinUIWindowContext.For(Window)` / `.Current`
- `ViewModelLifecycle.SetAuto`
- `WinUIFrameNavigator.Map<TViewModel, TView>`
- `WinUIDialogs` / `WinUINotifier` / `WinUIToastPresenter`
- `WinUIFormViewModel` (`INotifyDataErrorInfo`)

## Out of 1.0

Shell, `UseDeepLinks`, `UseSecureSessionAuth`, source generators, Reactive, CommunityToolkit compatibility, `NavigateForResultAsync`. IDE extensions are Phase 3.
