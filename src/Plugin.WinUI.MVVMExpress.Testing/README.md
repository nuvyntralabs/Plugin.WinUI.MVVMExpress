# Plugin.WinUI.MVVMExpress.Testing

Test fakes and leak probes for **MVVMExpress** ViewModels.

[![NuGet](https://img.shields.io/nuget/v/Plugin.WinUI.MVVMExpress.Testing.svg?label=NuGet)](https://www.nuget.org/packages/Plugin.WinUI.MVVMExpress.Testing)

```csharp
var nav = new FakeNavigator();
await nav.NavigateToAsync<HomeViewModel>();

await viewModel.AppearAsync();
await viewModel.DisappearAsync();

var leak = LeakProbe.Track(viewModel);
viewModel.Dispose();
Assert.True(LeakProbe.IsCollected(leak));
```

Also: `FakeDialogs`, `FakeMainThread`, `FakeConnectivity`, `FakeMessageHub`, `ScopedNavigator` (page-scope push/pop + dispose), `ScaleProfile` (Small / Mid / Large list sizes).

## Install

```bash
dotnet add package Plugin.WinUI.MVVMExpress.Testing
```

Target framework: `net10.0`. Depends on [Core](https://www.nuget.org/packages/Plugin.WinUI.MVVMExpress.Core). Reference from test projects only. Version `1.0.0`.

## Related

Product docs: [repository README](https://github.com/nuvyntralabs/Plugin.WinUI.MVVMExpress). Alternatives: xUnit + hand-rolled fakes, Prism.Maui testing helpers. License: MIT.
