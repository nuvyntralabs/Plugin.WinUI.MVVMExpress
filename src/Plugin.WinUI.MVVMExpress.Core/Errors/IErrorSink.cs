using Plugin.WinUI.MVVMExpress.Outcome;

namespace Plugin.WinUI.MVVMExpress.Errors;

/// <summary>Receives unexpected failures. Implementations must not swallow without logging or surfacing.</summary>
public interface IErrorSink
{
    /// <summary>Handles <paramref name="error"/>.</summary>
    /// <param name="error">Failure.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task HandleAsync(ErrorInfo error, CancellationToken cancellationToken = default);
}

/// <summary>No-op sink for tests and apps that have not registered a UI handler.</summary>
public sealed class NullErrorSink : IErrorSink
{
    /// <summary>Shared instance.</summary>
    public static NullErrorSink Instance { get; } = new();

    /// <inheritdoc />
    public Task HandleAsync(ErrorInfo error, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(error);
        return Task.CompletedTask;
    }
}
