# WinUI MVVMExpress IDE wrappers

Thin Visual Studio Code and Visual Studio extensions. They install [`Plugin.WinUI.MVVMExpress.Templates`](https://www.nuget.org/packages/Plugin.WinUI.MVVMExpress.Templates) and run `dotnet new`. The scaffold stays in [`templates/`](../templates/).

This is not the MAUI MVVMExpress extension.

| Host | Commands |
| --- | --- |
| Visual Studio Code | **WinUI MVVMExpress: Create New App**, **WinUI MVVMExpress: Add Page** |
| Visual Studio 2022+ | **Tools → WinUI MVVMExpress → Create New App…**, **Add Page…** |

After the template pack is installed, Visual Studio’s **File → New → Project** lists **MVVMExpress WinUI App** (`ide.host.json` on the project template). **Add → New Item** lists **MVVMExpress WinUI Page**.

Requires the .NET SDK on PATH. Extension version is `1.0.1`, same as Plugin.WinUI.MVVMExpress.

## Install from Marketplace

Search **WinUI MVVMExpress** and install:

| Host | Marketplace |
| --- | --- |
| Visual Studio Code | [WinUI MVVMExpress](https://marketplace.visualstudio.com/search?term=WinUI%20MVVMExpress&target=VSCode&category=All%20categories&sortBy=Relevance) |
| Visual Studio 2022+ | [WinUI MVVMExpress](https://marketplace.visualstudio.com/search?term=WinUI%20MVVMExpress&target=VS&category=All%20categories&vsVersion=&sortBy=Relevance) |

In the editor: **Extensions** → search **WinUI MVVMExpress** → **Install**. Then **WinUI MVVMExpress: Create New App** / **Add Page** (VS Code) or **Tools → WinUI MVVMExpress** (Visual Studio).

## Install (sideload)

Packed installers (version `1.0.1`) are in [`dist/`](dist/):

| Host | File | Install |
| --- | --- | --- |
| Visual Studio Code | `dist/nuvyntralabs.winui-mvvmexpress-1.0.1.vsix` | `code --install-extension extensions/dist/nuvyntralabs.winui-mvvmexpress-1.0.1.vsix` |
| Visual Studio 2022+ | `dist/nuvyntralabs.WinUIMVVMExpress.VisualStudio.1.0.1.vsix` | Double-click the `.vsix`, or **Extensions → Manage Extensions → Install from VSIX…** |

After Visual Studio install, the package loads in the background and installs `Plugin.WinUI.MVVMExpress.Templates`, so **File → New → Project** lists **MVVMExpress WinUI App**. **Tools → WinUI MVVMExpress** is present after install.

Rebuild both:

```bash
./extensions/pack.sh
```

Do not publish to the Marketplace from a local clone. CI uploads VSIX artifacts after **Version alignment** succeeds. Marketplace publish is manual.

## Pipeline

Library CI (`ci.yml`) packs NuGet only. It does not build VSIX files.

The **IDE extensions** workflow (`.github/workflows/ide-extensions.yml`) packs both wrappers. It runs on `main` / tags when `extensions/` changes, or from **Actions → IDE extensions → Run workflow**. It fails unless the extension version fields match [`Directory.Build.props`](../Directory.Build.props). Check locally:

```bash
python3 .github/scripts/check-versions.py --scope extensions
```

That run uploads the `.vsix` files as Actions artifacts. There is no Marketplace PAT in the workflow.

| Artifact | Listing |
| --- | --- |
| `vscode-WinUIMVVMExpress` | Visual Studio Code / Cursor — `nuvyntralabs.winui-mvvmexpress` |
| `vsix-WinUIMVVMExpress` | Visual Studio 2022+ — **WinUI MVVMExpress for Visual Studio** |

Open the run → **Artifacts** → download the VSIX → update the matching listing at [Marketplace manage](https://marketplace.visualstudio.com/manage). Do not create a new Visual Studio listing after the first publish.

Bump `Directory.Build.props` `Version` and the extension version fields together (the alignment job lists every file). Do not run `vsce publish` or `VsixPublisher` from a laptop.

## Visual Studio Code

```bash
./extensions/vscode/pack.sh
```

## Visual Studio

`pack-vsix.sh` compiles the `.vsct` with Wine on macOS or `VSCT.exe` on Windows, then writes the same Marketplace VSIX layout CI uploads.

```bash
./extensions/visualstudio/pack-vsix.sh
```

## CLI (no extension)

```bash
dotnet new install Plugin.WinUI.MVVMExpress.Templates
dotnet new winui-mvvmexpress -n MyApp
dotnet new winui-mvvmexpress-page -n Catalog --namespace MyApp
```

See [templates/README.md](../templates/README.md) and [getting started](../docs/getting-started.md).
