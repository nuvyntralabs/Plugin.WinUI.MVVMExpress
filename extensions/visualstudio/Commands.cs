using System.ComponentModel.Design;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace NuvyntraLabs.WinUIMVVMExpress.VisualStudio;

internal static class Commands
{
    public static readonly Guid CommandSet = new("d2e3f4a5-9c5e-4f02-8d3b-7e1c9a0f6b55");
    public const int CreateAppCommandId = 0x0100;
    public const int AddPageCommandId = 0x0101;

    public static async Task InitializeAsync(AsyncPackage package)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
        var commandService = await package.GetServiceAsync(typeof(IMenuCommandService)).ConfigureAwait(true) as OleMenuCommandService
            ?? throw new InvalidOperationException("IMenuCommandService is unavailable.");

        commandService.AddCommand(new OleMenuCommand((_, _) => Run(package, CreateAppAsync), new CommandID(CommandSet, CreateAppCommandId)));
        commandService.AddCommand(new OleMenuCommand((_, _) => Run(package, AddPageAsync), new CommandID(CommandSet, AddPageCommandId)));
    }

    private static void Run(AsyncPackage package, Func<AsyncPackage, Task> execute)
    {
        _ = package.JoinableTaskFactory.RunAsync(async () =>
        {
            try
            {
                await execute(package).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                ShowMessage(package, ex.Message, OLEMSGICON.OLEMSGICON_CRITICAL);
            }
        });
    }

    private static async Task CreateAppAsync(AsyncPackage package)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
        var name = Prompts.AskText(
            "WinUI MVVMExpress",
            "App name",
            DotnetTemplates.DefaultAppName,
            value => DotnetTemplates.IsValidTypeName(value) ? null : "Use a C# type name (letters, digits, underscore).");
        if (name is null)
        {
            return;
        }

        var parent = Prompts.AskFolder("Create the WinUI MVVMExpress app in this folder");
        if (parent is null)
        {
            return;
        }

        var outputDir = Path.Combine(parent, name);
        await DotnetTemplates.EnsureInstalledAsync(package.DisposalToken).ConfigureAwait(true);
        await DotnetTemplates.CreateAppAsync(name, outputDir, package.DisposalToken).ConfigureAwait(true);

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
        var solution = Path.Combine(outputDir, name + ".sln");
        if (File.Exists(solution) && await package.GetServiceAsync(typeof(DTE)).ConfigureAwait(true) is DTE dte)
        {
            dte.Solution.Open(solution);
            return;
        }

        ShowMessage(package, $"Created {name} at {outputDir}.", OLEMSGICON.OLEMSGICON_INFO);
    }

    private static async Task AddPageAsync(AsyncPackage package)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
        var name = Prompts.AskText(
            "WinUI MVVMExpress",
            "Page name (without Page suffix)",
            DotnetTemplates.DefaultPageName,
            value => DotnetTemplates.IsValidTypeName(value) ? null : "Use a C# type name (letters, digits, underscore).");
        if (name is null)
        {
            return;
        }

        var startFolder = Environment.CurrentDirectory;
        if (await package.GetServiceAsync(typeof(DTE)).ConfigureAwait(true) is DTE dte)
        {
            var solutionPath = dte.Solution?.FullName;
            if (!string.IsNullOrWhiteSpace(solutionPath))
            {
                var solutionDir = Path.GetDirectoryName(solutionPath);
                if (!string.IsNullOrWhiteSpace(solutionDir))
                {
                    startFolder = solutionDir;
                }
            }
        }

        var suggested = ProjectNamespaces.Detect(startFolder);
        var namespaceName = Prompts.AskText(
            "WinUI MVVMExpress",
            "Root namespace",
            suggested,
            value => DotnetTemplates.IsValidNamespace(value) ? null : "Enter a C# namespace.");
        if (namespaceName is null)
        {
            return;
        }

        var outputDir = Prompts.AskFolder("Add the WinUI MVVMExpress page in this folder", startFolder);
        if (outputDir is null)
        {
            return;
        }

        await DotnetTemplates.EnsureInstalledAsync(package.DisposalToken).ConfigureAwait(true);
        await DotnetTemplates.AddPageAsync(name, namespaceName, outputDir, package.DisposalToken).ConfigureAwait(true);

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
        ShowMessage(
            package,
            $"Created {name} page in {outputDir}. Call services.Add{name}() and map the route in App.xaml.cs.",
            OLEMSGICON.OLEMSGICON_INFO);
    }

    private static void ShowMessage(AsyncPackage package, string message, OLEMSGICON icon)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        VsShellUtilities.ShowMessageBox(
            package,
            message,
            "WinUI MVVMExpress",
            icon,
            OLEMSGBUTTON.OLEMSGBUTTON_OK,
            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
    }
}
