# Known limitations — Plugin.WinUI.MVVMExpress 1.0

- Native WinUI 3 / Windows App SDK only. MAUI-on-Windows stays on `Plugin.Maui.MVVMExpress`.
- No Shell. Use a `Frame` named `NavigationHost`.
- Toasts use `Popup` + `InfoBar` on the owner `XamlRoot`. They never replace `Window.Content`. If `XamlRoot` is null the toast is a no-op.
- Host / Navigation / Dialogs pack on Windows (`net10.0-windows10.0.19041.0`).
- Unpackaged (`WindowsPackageType=None`) is the supported sample path. Packaged MSIX is optional.
- Source generators, Reactive, CommunityToolkit compatibility, `UseDeepLinks`, and `UseSecureSessionAuth` are out of 1.0.
