# API parity — Maui 1.3 / WPF 1.0 → WinUI 3

| Surface | Status |
| --- | --- |
| Core ViewModel / commands / AsyncState / Outcome / messenger | Port |
| `INavigator` method surface | Port |
| `UseAuth<TChallenge>` / `GuardedNavigator` | Port |
| Validation / Pagination / Testing | Port |
| `UseWpfMvvmExpress` | Adapt → `UseWinUIMvvmExpress` |
| `DispatcherMainThread` | Adapt → `DispatcherQueueMainThread` |
| Lifecycle | Adapt → host attach/detach |
| `WpfFrameNavigator` | Adapt → `WinUIFrameNavigator` |
| Dialogs / toasts | Adapt → `WinUIDialogs` / overlay |
| `WinUIFormViewModel` | Adapt → `INotifyDataErrorInfo` |
| Shell / `UseShell` | Skip |
| `UseDeepLinks` / `UseSecureSessionAuth` | Skip |
| Source generators | Skip |
| Templates | Port — `winui-mvvmexpress` / `winui-mvvmexpress-page` |
| IDE extensions | Skip (Phase 3) |
