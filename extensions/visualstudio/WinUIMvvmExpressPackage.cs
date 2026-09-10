using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace NuvyntraLabs.WinUIMVVMExpress.VisualStudio;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[InstalledProductRegistration("WinUI MVVMExpress", "Create WinUI MVVMExpress apps and pages from Plugin.WinUI.MVVMExpress.Templates.", "1.0.1")]
[ProvideMenuResource("Menus.ctmenu", 1)]
[ProvideAutoLoad(UIContextGuids80.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
[Guid(WinUIMvvmExpressPackage.PackageGuidString)]
public sealed class WinUIMvvmExpressPackage : AsyncPackage
{
    public const string PackageGuidString = "c1d2e3f4-8b4d-4e91-9c2a-6f0d8e1b7a44";

    protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
        await Commands.InitializeAsync(this).ConfigureAwait(true);
        try
        {
            await DotnetTemplates.EnsureInstalledAsync(cancellationToken).ConfigureAwait(true);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            ActivityLog.LogWarning("WinUI MVVMExpress", ex.Message);
        }
    }
}
