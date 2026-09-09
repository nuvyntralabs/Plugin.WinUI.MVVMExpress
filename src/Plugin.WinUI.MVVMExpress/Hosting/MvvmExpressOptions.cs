namespace Plugin.WinUI.MVVMExpress.Hosting;

/// <summary>Host options for <c>UseWinUIMvvmExpress</c>.</summary>
public sealed class MvvmExpressOptions
{
    private readonly List<Action<global::Microsoft.Extensions.DependencyInjection.IServiceCollection>> _registrations = [];

    public bool CancelOperationsOnDisappear { get; set; }
    public bool EnableDiagnostics { get; set; }
    public bool MarshalNotifications { get; set; } = true;
    public bool AutoAttachLifecycle { get; set; } = true;
    public bool ConfirmDirtyNavigation { get; set; } = true;
    public bool ForwardNavigationFailures { get; set; } = true;
    public bool ApplyGeneratedRegistrations { get; set; } = true;
    internal Type? AuthChallengeViewModel { get; set; }

    public MvvmExpressOptions AddRegistration(Action<global::Microsoft.Extensions.DependencyInjection.IServiceCollection> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        _registrations.Add(configure);
        return this;
    }

    internal void ApplyRegistrations(global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        foreach (var configure in _registrations)
        {
            configure(services);
        }
    }
}
