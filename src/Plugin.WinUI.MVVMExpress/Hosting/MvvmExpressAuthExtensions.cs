using Plugin.WinUI.MVVMExpress.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.Hosting;

/// <summary>Host option to install <c>GuardedNavigator</c> without reconstructing it in app code.</summary>
public static class MvvmExpressAuthExtensions
{
    /// <summary>Sets the unauthenticated challenge ViewModel for <c>GuardedNavigator</c>.</summary>
    public static MvvmExpressOptions UseAuth<TChallenge>(this MvvmExpressOptions options)
        where TChallenge : class, IViewModel
    {
        ArgumentNullException.ThrowIfNull(options);
        options.AuthChallengeViewModel = typeof(TChallenge);
        return options;
    }
}
