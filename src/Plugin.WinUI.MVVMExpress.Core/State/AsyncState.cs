using Plugin.WinUI.MVVMExpress.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.State;

/// <summary>Bindable async result: status, data, and error flags.</summary>
/// <typeparam name="T">Payload type.</typeparam>
public sealed class AsyncState<T> : ObservableModel
{
    private ViewModelStatus _status = ViewModelStatus.Idle;
    private T? _data;
    private string? _error;
    private Exception? _exception;
    private DateTimeOffset? _timestamp;

    /// <summary>Current status.</summary>
    public ViewModelStatus Status
    {
        get => _status;
        private set
        {
            if (SetProperty(ref _status, value))
            {
                Notify(nameof(IsLoading));
                Notify(nameof(IsRefreshing));
                Notify(nameof(IsEmpty));
                Notify(nameof(HasError));
                Notify(nameof(IsSuccess));
            }
        }
    }

    /// <summary>Last successful payload.</summary>
    public T? Data
    {
        get => _data;
        private set => SetProperty(ref _data, value);
    }

    /// <summary>User-facing error text.</summary>
    public string? Error
    {
        get => _error;
        private set => SetProperty(ref _error, value);
    }

    /// <summary>Last exception, if any.</summary>
    public Exception? Exception
    {
        get => _exception;
        private set => SetProperty(ref _exception, value);
    }

    /// <summary>When the last transition completed.</summary>
    public DateTimeOffset? Timestamp
    {
        get => _timestamp;
        private set => SetProperty(ref _timestamp, value);
    }

    /// <summary>Gets a value indicating whether status is <see cref="ViewModelStatus.Loading"/>.</summary>
    public bool IsLoading => Status == ViewModelStatus.Loading;

    /// <summary>Gets a value indicating whether status is <see cref="ViewModelStatus.Refreshing"/>.</summary>
    public bool IsRefreshing => Status == ViewModelStatus.Refreshing;

    /// <summary>Gets a value indicating whether status is <see cref="ViewModelStatus.Empty"/>.</summary>
    public bool IsEmpty => Status == ViewModelStatus.Empty;

    /// <summary>Gets a value indicating whether status is <see cref="ViewModelStatus.Error"/>.</summary>
    public bool HasError => Status == ViewModelStatus.Error;

    /// <summary>Gets a value indicating whether status is <see cref="ViewModelStatus.Success"/>.</summary>
    public bool IsSuccess => Status == ViewModelStatus.Success;

    /// <summary>Loads data as an initial request.</summary>
    public Task<T> LoadAsync(Func<CancellationToken, Task<T>> loader, CancellationToken cancellationToken = default)
        => RunAsync(ViewModelStatus.Loading, loader, cancellationToken);

    /// <summary>Reloads data while keeping the previous <see cref="Data"/> until the new value arrives.</summary>
    public Task<T> RefreshAsync(Func<CancellationToken, Task<T>> loader, CancellationToken cancellationToken = default)
        => RunAsync(ViewModelStatus.Refreshing, loader, cancellationToken);

    private async Task<T> RunAsync(
        ViewModelStatus running,
        Func<CancellationToken, Task<T>> loader,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(loader);
        Status = running;
        Error = null;
        Exception = null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var result = await loader(cancellationToken).ConfigureAwait(false);
            Data = result;
            Timestamp = DateTimeOffset.UtcNow;
            Status = IsEmptyPayload(result) ? ViewModelStatus.Empty : ViewModelStatus.Success;
            return result;
        }
        catch (OperationCanceledException)
        {
            Status = ViewModelStatus.Cancelled;
            throw;
        }
        catch (Exception ex)
        {
            Exception = ex;
            Error = ex.Message;
            Timestamp = DateTimeOffset.UtcNow;
            Status = ViewModelStatus.Error;
            throw;
        }
    }

    private static bool IsEmptyPayload(T result)
        => result is null
           || result is System.Collections.ICollection { Count: 0 }
           || result is Array { Length: 0 };
}
