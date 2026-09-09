# Plugin.WinUI.MVVMExpress — design plan

**Status:** Plan only. No repository, packages, or submodule yet.  
**Product:** MVVMExpress (WinUI 3 family)  
**Package prefix:** `Plugin.WinUI.MVVMExpress`  
**Catalog slug:** `plugin-winui-mvvmexpress`  
**Closest shipped reference:** [Plugin.Wpf.MVVMExpress](https://github.com/nuvyntralabs/Plugin.Wpf.MVVMExpress) 1.0 (Frame host) and [Plugin.Maui.MVVMExpress](https://github.com/nuvyntralabs/Plugin.Maui.MVVMExpress) 1.3 (Core contract).

This family is for **native WinUI 3 / Windows App SDK** desktop apps. It is **not** a Windows TFM of `Plugin.Maui.MVVMExpress` (that package already hosts MAUI-on-Windows). It is **not** Uno Platform (see [plugin-uno-mvvmexpress.md](plugin-uno-mvvmexpress.md)).

Usual alternative when only properties and commands are needed: [CommunityToolkit.Mvvm](https://www.nuget.org/packages/CommunityToolkit.Mvvm).

---

## 1. Problem

A production WinUI 3 app needs more than `INotifyPropertyChanged` and `ICommand`:

- ViewModel lifecycle bound to `Page` / `Window` Loaded–Unloaded
- Async work with cancellation, timeout, retry, concurrency, and bindable busy state
- Strongly typed navigation that does not call `Frame.Navigate` from a ViewModel
- Testable ViewModels with no `Microsoft.UI.Xaml` statics
- Dialogs and toasts that resolve an owner `Window` / `XamlRoot` on the UI thread

CommunityToolkit.Mvvm covers properties, commands, and messaging. WinUI `Frame` covers page stacks. Neither is an application shell with typed args, auth replace-root, or a host-agnostic `INavigator`.

## 2. Principles

1. **Core is UI-framework-free.** `Plugin.WinUI.MVVMExpress.Core` targets `net10.0` only. No `Microsoft.WindowsAppSDK`, no `Microsoft.UI.Xaml`, no WPF, no MAUI.
2. **Independent family.** Do not add a `PackageReference` to `Plugin.Maui.MVVMExpress.*`, `Plugin.Wpf.MVVMExpress.*`, or `Plugin.Uno.MVVMExpress.*`. Core shapes match Maui 1.3 / WPF 1.0 so a ViewModel ports with a namespace swap.
3. **Interfaces at the ViewModel boundary.** ViewModels depend on `INavigator`, `IDialogs`, `IMainThread`, `IAuthState` — never `Frame`, `ContentDialog`, `XamlRoot`, or `DispatcherQueue` statics.
4. **One navigator per `Window`.** Use `IWindowContext` / `WindowNavigatorRegistry`. Multi-window is first-class (same as WPF).
5. **No Shell.** WinUI has no MAUI Shell. `Frame` + `SectionHostViewModel` cover stack and in-place tabs / `NavigationView`.
6. **Toasts must not replace `Window.Content`.** Draw on a `Popup` / overlay attached to `XamlRoot`.
7. **No sibling MauiEssentials `PackageReference`.** Apps may compose shared `net10.0` libraries themselves.
8. **AOT / trim is a constraint, not a v1 promise.** Prefer typed `Map<TViewModel, TView>()` over reflection. Do not enable `PublishTrimmed` on samples until a dedicated pass exists (WinUI XAML trimming is unsafe by default).
9. **Pipeline-only publish.** CI pushes nuget.org and GitHub Packages. Never `dotnet nuget push` from this workspace.

## 3. Naming

| Role | Value |
| --- | --- |
| Product | MVVMExpress (WinUI family) |
| NuGet / assembly prefix | `Plugin.WinUI.MVVMExpress` |
| Root namespace | `Plugin.WinUI.MVVMExpress` |
| Host registration | `UseWinUIMvvmExpress` / `AddWinUIMvvmExpress` |
| Navigation | `UseFrameNavigation` → `WinUIFrameNavigator` |
| Dialogs | `UseDialogs` → `WinUIDialogs` / `WinUINotifier` |
| Template | `dotnet new winui-mvvmexpress` |
| GitHub | `nuvyntralabs/Plugin.WinUI.MVVMExpress` |
| Hub submodule folder | `WinUIMVVMExpress` |

Type names stay collision-free with CommunityToolkit.Mvvm (`ObservableModel`, `AsyncModelCommand`, `INavigator`, `Outcome`).

## 4. Target frameworks

| Package | TFM | Notes |
| --- | --- | --- |
| Core, Validation, Pagination, Testing, Templates | `net10.0` | Pack on Linux |
| Host, Navigation, Dialogs | `net10.0-windows10.0.19041.0` | Windows App SDK / WinUI 3. Pack on `windows-latest` |

Use Windows 10 2004 (`19041`) — the same OS bar as MAUI Windows — not WPF’s `17763`. Set `<SupportedOSPlatformVersion>10.0.17763.0</SupportedOSPlatformVersion>` if the Windows App SDK package requires a lower min version.

Host projects:

```xml
<TargetFramework>net10.0-windows10.0.19041.0</TargetFramework>
<UseWinUI>true</UseWinUI>
```

Samples must run **unpackaged** (`WindowsPackageType=None`) and **packaged** (MSIX). Unpackaged is the CI smoke path.

Do **not** take the umbrella `Microsoft.WindowsAppSDK` if a slimmer `Microsoft.WindowsAppSDK.WinUI` + runtime pair is enough. Record the chosen package set in `Directory.Packages.props`.

## 5. Packages (1.0)

```
Plugin.WinUI.MVVMExpress.Core         net10.0
    ▲
Plugin.WinUI.MVVMExpress              Host (DI, DispatcherQueue, lifecycle)
    ▲
    ├── Navigation                    WinUIFrameNavigator
    └── Dialogs                       ContentDialog + XamlRoot overlay toast

Plugin.WinUI.MVVMExpress.Validation   net10.0
Plugin.WinUI.MVVMExpress.Pagination   net10.0
Plugin.WinUI.MVVMExpress.Testing      net10.0
Plugin.WinUI.MVVMExpress.Templates    net10.0   (Phase 3)
```

| Package | Role |
| --- | --- |
| `.Core` | ViewModels, commands, `AsyncState`, `Outcome`, `INavigator` abstractions, `InMemoryNavigator`, `GuardedNavigator`, forms, auth attributes |
| Host | `UseWinUIMvvmExpress`, `DispatcherQueueMainThread`, `WinUIWindowContext`, Loaded/Unloaded lifecycle, `WinUIFormViewModel` (`INotifyDataErrorInfo`) |
| `.Navigation` | `WinUIFrameNavigator.Map<TViewModel, TView>()` |
| `.Dialogs` | `WinUIDialogs` (`ContentDialog`), `WinUINotifier` (`Popup` + `InfoBar`) |
| `.Validation` / `.Pagination` / `.Testing` | Port of WPF packages |
| `.Templates` | `winui-mvvmexpress` / `winui-mvvmexpress-page` |

**Not in 1.0:** source generators, Reactive, CommunityToolkit compatibility, `UseDeepLinks`, `UseSecureSessionAuth`, Prism-style regions, `NavigateForResultAsync`.

## 6. API parity — Maui 1.3 / WPF 1.0 → WinUI

| Surface | Status |
| --- | --- |
| Core ViewModel / commands / `AsyncState` / `Outcome` / messenger | Port |
| `INavigator` method surface | Port |
| `UseAuth<TChallenge>` / `GuardedNavigator` | Port |
| Validation / Pagination / Testing | Port |
| `UseWpfMvvmExpress` | Adapt → `UseWinUIMvvmExpress` |
| `DispatcherMainThread` | Adapt → `DispatcherQueueMainThread` (`Microsoft.UI.Dispatching.DispatcherQueue`) |
| WPF Loaded / Unloaded attached property | Adapt → `FrameworkElement.Loaded` / `Unloaded` attached property |
| `WpfFrameNavigator` | Adapt → `WinUIFrameNavigator` (`Microsoft.UI.Xaml.Controls.Frame`) |
| Modal stack | Adapt → `ContentDialog` (in-window) or a second `Microsoft.UI.Xaml.Window` |
| Multi-window | Enhance — first-class `IWindowContext` per `Window` (WPF already does this) |
| `WpfDialogs` / `MessageBox` | Adapt → `ContentDialog` with `XamlRoot` from the owner window |
| Toast overlay | Adapt → `Popup` + `InfoBar` on `XamlRoot`. Never replace `Window.Content` |
| Form XAML `INotifyDataErrorInfo` | Adapt → `WinUIFormViewModel` |
| `SectionHostViewModel` | Port — bind to `NavigationView` / `SelectorBar` in the sample |
| Shell / `UseShell` | Skip |
| `UseDeepLinks` / `UseSecureSessionAuth` | Skip |
| MAUI KeyboardManager / LeakAnalyser / native plugins | Skip |
| Source generators | Skip (1.0) |
| Templates / VS Code / Visual Studio extensions | Port in Phase 3 — wrappers call `dotnet new` only |

## 7. Host mapping

### 7.1 Registration

```csharp
var builder = Host.CreateApplicationBuilder();
builder.Services.AddSingleton<IAuthState, DemoAuthState>();
builder.Services.UseWinUIMvvmExpress(o => o
    .UseFrameNavigation((nav, _) => nav
        .Map<LoginViewModel, LoginPage>("login")
        .Map<HomeViewModel, HomePage>("home"))
    .UseDialogs()
    .UseAuth<LoginViewModel>());
builder.Services.AddSingleton<MainWindow>();
var host = builder.Build();
await host.StartAsync();
```

`App.OnLaunched` shows `MainWindow` and sets `WinUIWindowContext.Current`. Do not construct views inside a ViewModel.

### 7.2 Window contract

Put a `Frame` named `NavigationHost` in `MainWindow`. Keep a sibling overlay host (empty `Grid` or `Canvas`) **above** the frame for toasts, or let `WinUINotifier` attach a `Popup` to `window.Content.XamlRoot`.

```xml
<Window …>
  <Grid>
    <Frame x:Name="NavigationHost" />
    <!-- optional named overlay; toast presenter must still work if omitted -->
  </Grid>
</Window>
```

After sign-in, `ResetAsync<HomeViewModel>()` replaces the Frame journal so Back cannot return to login.

### 7.3 Threading

- `DispatcherQueueMainThread` wraps `DispatcherQueue.GetForCurrentThread()` captured at host start, with a fallback to the window’s dispatcher.
- Navigators hop to `IMainThread` before `Activator` / DI construction of a `Page`.
- `ContentDialog.ShowAsync` and `Popup.IsOpen` run on that dispatcher.
- Library code uses `ConfigureAwait(false)` except immediately before UI mutation.

### 7.4 Lifecycle

Attached property `ViewModelLifecycle.SetAuto` on the page (or window content):

```
Loaded  → InitializeAsync (once) → OnNavigatedToAsync → OnAppearingAsync
Unloaded → OnDisappearingAsync → OnNavigatedFromAsync
Closed / dispose → cancel ViewModelCancellationToken
```

Match WPF: dispose is the guaranteed cancel path. `CancelOperationsOnDisappear` may exist on options without cancelling the token yet.

### 7.5 Navigation

`WinUIFrameNavigator` implements `IPageNavigator`:

| `INavigator` | WinUI host |
| --- | --- |
| `NavigateToAsync<T>` | `Frame.Navigate(viewType)` after constructing the page and setting `DataContext` |
| `GoBackAsync` | `Frame.GoBack()` + pop `NavigationStack` |
| `ReplaceAsync` | navigate and remove the previous journal entry |
| `ResetAsync` | `Frame.BackStack.Clear()` then navigate (login → home) |
| `PopToRootAsync` | pop until one entry remains |
| Modal | `ContentDialog` hosting a mapped view, or a owned `Window` |

Journal and `NavigationStack` must stay aligned. Do not call `Frame.Navigate` from a ViewModel.

`Map<TViewModel, TView>()` requires `TView : Microsoft.UI.Xaml.FrameworkElement`.

### 7.6 Dialogs and toasts

| Abstraction | WinUI implementation | Rule |
| --- | --- | --- |
| `IDialogs.AlertAsync` / `ConfirmAsync` | `ContentDialog` | Set `XamlRoot` from the owner window. Hop to `IMainThread`. |
| `IDialogs.ErrorAsync` | Same as alert | Title `"Error"`, message from `ErrorInfo` |
| `INotifier.ToastAsync` | `Popup` + `InfoBar` (non-closable or auto-dismiss) | Must not wrap or replace `Window.Content` |

If `XamlRoot` is null (window not activated), fail with `Outcome` / no-op toast — do not throw a COM exception to the ViewModel.

### 7.7 Forms

`WinUIFormViewModel` implements `INotifyDataErrorInfo` on top of Core `FormViewModel`. Bind `TextBox` with `UpdateSourceTrigger=PropertyChanged` where WinUI supports it. No `Validation.For` attached property in 1.0 unless it ports cheaply from WPF.

## 8. What stays in Core vs WinUI

| Independent (Core / Validation / Pagination / Testing) | WinUI-specific (Host / Navigation / Dialogs) |
| --- | --- |
| `ObservableModel`, commands, state, outcome | Lifecycle attached property |
| Messaging, busy, retry, timeout | `DispatcherQueueMainThread` |
| Forms, dirty, undo, collections | `Frame` host |
| Pagination, search | `ContentDialog` / `Popup` toast |
| Validation engine | View/ViewModel map |
| `INavigator` / `InMemoryNavigator` / `GuardedNavigator` | `WinUIWindowContext` |

## 9. Phases

### Phase 0 — Design lock (this document)

- [ ] Product prefix `Plugin.WinUI.MVVMExpress` approved
- [ ] TFM `net10.0-windows10.0.19041.0` approved
- [ ] Independent-family rule approved (no Maui / WPF package refs)
- [ ] Create empty repo `nuvyntralabs/Plugin.WinUI.MVVMExpress` + hub submodule (no NuGet yet)

**Exit:** repo exists with `ARCHITECTURE.md` / `API-DESIGN.md` / `API-PARITY.md` copied from this plan.

### Phase 1 — Core + Host (0.1.0-preview)

Port WPF Core (namespace `Plugin.WinUI.MVVMExpress`). Implement:

- `UseWinUIMvvmExpress` / `AddWinUIMvvmExpress`
- `DispatcherQueueMainThread`
- `WinUIWindowContext.For(Window)` / `.Current`
- `ViewModelLifecycle` Loaded / Unloaded

**Acceptance**

- `dotnet test` on Core / Validation / Pagination / Testing passes on `net10.0` (Linux CI)
- Host builds on Windows for `net10.0-windows10.0.19041.0`
- A test ViewModel runs lifecycle + `AsyncState` + dispose-cancel + `WeakReference` GC
- `dotnet list package` on Core shows no Windows App SDK reference

### Phase 2 — Navigation + Dialogs (0.1.1-preview)

- `WinUIFrameNavigator` + `UseFrameNavigation`
- `UseAuth<TChallenge>()` + `ResetAsync` after login
- `WinUIDialogs` / `WinUINotifier`
- Playground: login replace-root, list, form, toast, second window

**Acceptance**

- `InMemoryNavigator` guard test (already in Core) still passes
- Playground: Back after login does not return to the login page
- Toast visible without replacing `MainWindow.Content`
- `ContentDialog` uses the owner `XamlRoot`
- View construction asserted on the UI thread (same test idea as MAUI / WPF navigators)

### Phase 3 — Productization (1.0.0)

- `dotnet new winui-mvvmexpress` / `winui-mvvmexpress-page`
- VS Code / Visual Studio extensions that only install the template pack and run `dotnet new`
- Getting started, `llms.txt`, `AGENTS.md`
- Hub catalog row, `docs/packages/README.md`, skill catalog
- CI: Linux pack Core/Validation/Pagination/Testing/Templates; Windows pack Host/Navigation/Dialogs; nuget.org via `NUGET_KEY_WINUI`; GitHub Packages via `GITHUB_TOKEN`

**Acceptance:** same Definition of Done as WPF 1.0 — 15-minute Playground path, SemVer lock, version-align script.

## 10. Repository layout

```
Plugin.WinUI.MVVMExpress/
├── AGENTS.md
├── ARCHITECTURE.md
├── API-DESIGN.md
├── API-PARITY.md
├── CHANGELOG.md
├── README.md
├── llms.txt
├── Directory.Build.props
├── Directory.Packages.props
├── src/
│   ├── Plugin.WinUI.MVVMExpress.Core
│   ├── Plugin.WinUI.MVVMExpress
│   ├── Plugin.WinUI.MVVMExpress.Navigation
│   ├── Plugin.WinUI.MVVMExpress.Dialogs
│   ├── Plugin.WinUI.MVVMExpress.Validation
│   ├── Plugin.WinUI.MVVMExpress.Pagination
│   ├── Plugin.WinUI.MVVMExpress.Testing
│   └── Plugin.WinUI.MVVMExpress.Templates
├── tests/
├── samples/Playground
├── templates/
├── extensions/          # Phase 3 — call dotnet new only
└── .github/workflows/ci.yml
```

Copy WPF `check-versions.py` and the hub `check-nuget-release.py` / `push-nuget-packages.py` pattern. Scope nuget.org key to `Plugin.WinUI.*` as secret `NUGET_KEY_WINUI`. `Plugin.Maui.*` keeps `NUGET_KEY`. `Plugin.Wpf.*` keeps `NUGET_KEY_WPF`.

## 11. Playground (Phase 2)

Mirror WPF Playground:

- Demo auth: `demo@mvvmexpress.dev` / `secret`
- Login → `ResetAsync<HomeViewModel>()`
- List (`PagedCollection` or `SnapshotCollection`)
- Form with dirty guard
- Toast button
- “Open window” that registers a second `IWindowContext`

Do not call `ContentDialog.ShowAsync` or `Frame.Navigate` from a ViewModel.

## 12. Risks

| Risk | Mitigation |
| --- | --- |
| Confused with MAUI Windows | README first paragraph: native WinUI 3 only; MAUI apps stay on `Plugin.Maui.MVVMExpress` |
| Confused with Uno | Separate prefix; Uno plan forbids referencing this nupkg |
| `ContentDialog` without `XamlRoot` | Resolve from `WinUIWindowContext`; no-op / `Outcome` if missing |
| Unpackaged `REGDB_E_CLASSNOTREG` | Sample sets `WindowsPackageType=None`; document packaged vs unpackaged |
| Toast replaces content | Overlay/`Popup` only; test asserts `Window.Content` type is unchanged |
| UI thread COM | Navigator + dialogs always hop to `IMainThread` first |
| Sharing Core with WPF via project reference | Forbidden. Port files; independent SemVer |

## 13. Decision log

| ID | Decision | Status |
| --- | --- | --- |
| W1 | Official prefix `Plugin.WinUI.MVVMExpress` | Proposed |
| W2 | Independent family — no Maui / WPF / Uno package refs | Proposed |
| W3 | Host TFM `net10.0-windows10.0.19041.0` | Proposed |
| W4 | Frame named `NavigationHost`; no Shell | Proposed |
| W5 | Dialogs = `ContentDialog`; toasts = `Popup` + `InfoBar` | Proposed |
| W6 | `NUGET_KEY_WINUI` for nuget.org | Proposed |
| W7 | Source generators and Reactive out of 1.0 | Proposed |
| W8 | Implement this family before Uno (Uno copies WinUI host patterns) | Proposed |

## 14. How to use this document

This file is the seed for `ARCHITECTURE.md`, `API-DESIGN.md`, `API-PARITY.md`, and `DESIGN-PLAN.md` in the new repo. Do not generate the entire framework in one change. Phase 1 first. Publishing stays in GitHub Actions.
