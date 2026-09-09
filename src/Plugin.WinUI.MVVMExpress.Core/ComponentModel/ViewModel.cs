using Plugin.WinUI.MVVMExpress.Busy;
using Plugin.WinUI.MVVMExpress.Composition;
using Plugin.WinUI.MVVMExpress.Errors;
using Plugin.WinUI.MVVMExpress.Outcome;
using Plugin.WinUI.MVVMExpress.State;
using Plugin.WinUI.MVVMExpress.Threading;
using Result = Plugin.WinUI.MVVMExpress.Outcome.Outcome;

namespace Plugin.WinUI.MVVMExpress.ComponentModel;

/// <summary>
/// ViewModel with lifecycle hooks, a lifetime <see cref="CancellationToken"/>, and dispose that cancels in-flight work.
/// </summary>
public abstract class ViewModel : ObservableModel, IViewModel, IViewModelComposer
{
    private readonly CancellationTokenSource _lifetime;
    private readonly CancellationToken _lifetimeToken;
    private readonly List<IViewModel> _children = [];
    private ViewModelStatus _status = ViewModelStatus.Idle;
    private bool _disposed;

    /// <summary>Creates a ViewModel and captures a lifetime token that stays readable after dispose.</summary>
    /// <param name="errors">Optional unexpected-error sink.</param>
    /// <param name="busy">Optional nested busy gate used by <see cref="ExecuteAsync"/>.</param>
    /// <param name="mainThread">Optional UI dispatcher for property notifications.</param>
    protected ViewModel(IErrorSink? errors = null, IBusyGate? busy = null, IMainThread? mainThread = null)
    {
        _lifetime = new CancellationTokenSource();
        _lifetimeToken = _lifetime.Token;
        Errors = errors ?? NullErrorSink.Instance;
        Busy = busy;
        if (mainThread is not null)
        {
            NotificationThread = mainThread;
        }
    }

    /// <summary>Unexpected-error sink.</summary>
    protected IErrorSink Errors { get; }

    /// <summary>Optional busy gate.</summary>
    protected IBusyGate? Busy { get; }

    /// <summary>Runs <paramref name="operation"/> with lifetime cancel, busy, and <see cref="Result"/>.</summary>
    /// <param name="operation">Work to run.</param>
    /// <param name="cancellationToken">Caller token.</param>
    protected async Task<Result> ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        using var scope = Busy?.Enter();
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, ViewModelCancellationToken);
        try
        {
            await operation(linked.Token).ConfigureAwait(false);
            return Result.Success();
        }
        catch (OperationCanceledException)
        {
            Status = ViewModelStatus.Cancelled;
            return Result.Failure("E_CANCEL", "Cancelled");
        }
        catch (Exception ex)
        {
            Status = ViewModelStatus.Error;
            var error = new ErrorInfo("E_OP", ex.Message, ex);
            await Errors.HandleAsync(error, linked.Token).ConfigureAwait(false);
            return Result.Failure(error);
        }
    }

    /// <inheritdoc />
    public ViewModelStatus Status
    {
        get => _status;
        protected set
        {
            if (SetProperty(ref _status, value))
            {
                Notify(nameof(IsBusy));
            }
        }
    }

    /// <inheritdoc />
    public virtual bool IsBusy => Status is ViewModelStatus.Loading
        or ViewModelStatus.Refreshing
        or ViewModelStatus.Saving;

    /// <summary>Token cancelled when the ViewModel is disposed. Safe to read after <see cref="Dispose()"/>.</summary>
    public CancellationToken ViewModelCancellationToken => _lifetimeToken;

    /// <summary>Gets a value indicating whether <see cref="Dispose()"/> has run.</summary>
    public bool IsDisposed => _disposed;

    /// <inheritdoc />
    public IReadOnlyList<IViewModel> Children => _children;

    /// <inheritdoc />
    public TChild Attach<TChild>(TChild child)
        where TChild : class, IViewModel
    {
        ArgumentNullException.ThrowIfNull(child);
        ObjectDisposedException.ThrowIf(_disposed, this);
        _children.Add(child);
        return child;
    }

    /// <summary>Initializes attached children.</summary>
    protected async Task InitializeChildrenAsync(CancellationToken cancellationToken = default)
    {
        foreach (var child in _children)
        {
            await child.InitializeAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Forwards appear to attached children.</summary>
    protected async Task AppearChildrenAsync(CancellationToken cancellationToken = default)
    {
        foreach (var child in _children)
        {
            await child.OnAppearingAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Forwards disappear to attached children.</summary>
    protected async Task DisappearChildrenAsync(CancellationToken cancellationToken = default)
    {
        foreach (var child in _children)
        {
            await child.OnDisappearingAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Called once after construction when the host is ready.</summary>
    /// <param name="cancellationToken">Linked with <see cref="ViewModelCancellationToken"/> by the host.</param>
    public virtual Task InitializeAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>Called when the corresponding page appears.</summary>
    public virtual Task OnAppearingAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>Called when the corresponding page disappears.</summary>
    public virtual Task OnDisappearingAsync(CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    /// <summary>Releases the lifetime token. Override to dispose child ViewModels.</summary>
    /// <param name="disposing"><see langword="true"/> when called from <see cref="Dispose()"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            foreach (var child in _children)
            {
                child.Dispose();
            }

            _children.Clear();

            try
            {
                _lifetime.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Already disposed by a concurrent caller.
            }

            _lifetime.Dispose();
        }

        _disposed = true;
    }
}
