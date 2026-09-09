# Plugin.WinUI.MVVMExpress.Core

UI-framework-free MVVM primitives for **.NET**. No WPF or MAUI reference.

`ObservableModel`, `ViewModel` lifecycle, async commands, `AsyncState<T>`, `Outcome`, messaging, `INavigator`, `ICache`, and `IAuthState`.

```csharp
public sealed class HomeViewModel : ViewModel
{
  public AsyncState<IReadOnlyList<Product>> Products { get; } = new();
  public AsyncModelCommand RefreshCommand { get; }

  public HomeViewModel(ICatalog catalog)
  {
    RefreshCommand = new AsyncModelCommand(
      ct => Products.LoadAsync(token => catalog.ListAsync(token), ct));
  }
}
```

## Install

```bash
dotnet add package Plugin.WinUI.MVVMExpress.Core
```

Target framework: `net10.0`. Version `1.0.0`.

```csharp
services.AddMvvmExpress(); // tests and shared libraries
```

WPF apps also add [Plugin.WinUI.MVVMExpress](https://www.nuget.org/packages/Plugin.WinUI.MVVMExpress) and call `UseWinUIMvvmExpress()`.

Do not call `MessageBox.Show` from a ViewModel. Use `IDialogs` / `INavigator`.

License: MIT. Niladri Padhy / MauiEssentials.
