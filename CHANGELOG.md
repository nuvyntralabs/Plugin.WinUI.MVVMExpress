# Changelog

## 0.1.0-preview

- Independent WinUI 3 family (no sibling MVVMExpress package references).
- Core, Validation, Pagination, Testing ported to `Plugin.WinUI.MVVMExpress` namespaces.
- Host: `UseWinUIMvvmExpress`, `DispatcherQueueMainThread`, `WinUIWindowContext`, lifecycle.
- Navigation: `WinUIFrameNavigator`, `UseAuth<TChallenge>()`.
- Dialogs: `WinUIDialogs` and overlay toasts.
- Playground sample: login replace-root, list, form, toast, second window.
- Templates: `dotnet new winui-mvvmexpress` / `winui-mvvmexpress-page`.
