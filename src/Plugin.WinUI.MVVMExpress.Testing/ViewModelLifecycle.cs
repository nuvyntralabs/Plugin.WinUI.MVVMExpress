using System.Runtime.CompilerServices;
using Plugin.WinUI.MVVMExpress.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.Testing;

/// <summary>
/// Drives <see cref="IViewModel"/> appear / disappear without a MAUI page or <c>ViewModelLifecycleBehavior</c>.
/// First <see cref="AppearAsync"/> calls <see cref="IViewModel.InitializeAsync"/> once.
/// </summary>
public static class ViewModelLifecycle
{
    private static readonly ConditionalWeakTable<IViewModel, object> Initialized = [];

    /// <summary>
    /// Initializes on the first call, then runs <see cref="IViewModel.OnAppearingAsync"/>.
    /// </summary>
    /// <param name="viewModel">ViewModel under test.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    public static async Task AppearAsync(this IViewModel viewModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        if (!Initialized.TryGetValue(viewModel, out _))
        {
            Initialized.Add(viewModel, Sentinel.Instance);
            await viewModel.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }

        await viewModel.OnAppearingAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Forwards <see cref="IViewModel.OnDisappearingAsync"/>.</summary>
    /// <param name="viewModel">ViewModel under test.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    public static Task DisappearAsync(this IViewModel viewModel, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        return viewModel.OnDisappearingAsync(cancellationToken);
    }

    private sealed class Sentinel
    {
        internal static readonly Sentinel Instance = new();
    }
}
