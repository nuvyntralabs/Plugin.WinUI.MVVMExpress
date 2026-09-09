using System.Windows.Input;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Input;

/// <summary>
/// Async command with running state, cancellation, and single-flight execution (prevent concurrent by default).
/// </summary>
public sealed class AsyncModelCommand : ObservableModel, ICommand
{
    private readonly Func<CancellationToken, Task> _execute;
    private readonly Func<bool>? _canExecute;
    private readonly AsyncCommandOptions _options;
    private readonly CommandPipeline _pipeline;
    private readonly WeakCanExecuteChanged _canExecuteChanged = new();
    private CancellationTokenSource? _execution;
    private int _runLock;
    private CommandExecutionState _state = CommandExecutionState.Idle;
    private bool _isRunning;

    /// <summary>Creates an async command.</summary>
    /// <param name="execute">Work to run.</param>
    /// <param name="canExecute">Optional predicate.</param>
    /// <param name="options">Timeout, retry, and concurrency.</param>
    public AsyncModelCommand(
        Func<CancellationToken, Task> execute,
        Func<bool>? canExecute = null,
        AsyncCommandOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(execute);
        _execute = execute;
        _canExecute = canExecute;
        _options = options ?? new AsyncCommandOptions();
        _pipeline = new CommandPipeline(_options);
        if (_options.MainThread is not null)
        {
            NotificationThread = _options.MainThread;
        }
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged
    {
        add => _canExecuteChanged.Add(value);
        remove => _canExecuteChanged.Remove(value);
    }

    /// <summary>Gets a value indicating whether the command is executing.</summary>
    public bool IsRunning
    {
        get => _isRunning;
        private set => SetProperty(ref _isRunning, value);
    }

    /// <summary>Gets the last / current execution state.</summary>
    public CommandExecutionState State
    {
        get => _state;
        private set => SetProperty(ref _state, value);
    }

    /// <summary>Gets a value indicating whether cancellation was requested for the current run.</summary>
    public bool IsCancellationRequested => _execution?.IsCancellationRequested == true;

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        if (IsRunning && !_pipeline.AllowsExecuteWhileRunning)
        {
            return false;
        }

        return _canExecute?.Invoke() ?? true;
    }

    /// <inheritdoc />
    public async void Execute(object? parameter)
    {
        try
        {
            await ExecuteAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await CommandFailure.HandleAsync(ex, _options).ConfigureAwait(false);
        }
    }

    /// <summary>Executes the command and observes cancellation.</summary>
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        if (_canExecute?.Invoke() == false)
        {
            return;
        }

        if (!await _pipeline.WaitPolicyAsync(cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        if (_pipeline.InterruptsPrevious && IsRunning)
        {
            Cancel();
        }
        else if (IsRunning && !_pipeline.AllowsExecuteWhileRunning)
        {
            return;
        }

        await _pipeline.EnterQueueAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!_pipeline.AllowsOverlap && Interlocked.CompareExchange(ref _runLock, 1, 0) != 0)
            {
                if (!_pipeline.InterruptsPrevious)
                {
                    return;
                }

                while (Interlocked.CompareExchange(ref _runLock, 1, 0) != 0)
                {
                    await Task.Yield();
                }
            }

            await RunCoreAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _pipeline.ExitQueue();
        }
    }

    private async Task RunCoreAsync(CancellationToken cancellationToken)
    {
        CancellationTokenSource? linked = null;
        try
        {
            linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (_options.Timeout is { } timeout)
            {
                linked.CancelAfter(timeout);
            }

            _execution = linked;
            IsRunning = true;
            State = CommandExecutionState.Running;
            NotifyCanExecuteChanged();
            var attempts = 0;
            while (true)
            {
                try
                {
                    await _execute(linked.Token).ConfigureAwait(false);
                    State = CommandExecutionState.Completed;
                    break;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch when (attempts < _options.RetryCount)
                {
                    attempts++;
                    if (_options.RetryDelay > TimeSpan.Zero)
                    {
                        await Task.Delay(_options.RetryDelay, linked.Token).ConfigureAwait(false);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            State = CommandExecutionState.Cancelled;
        }
        catch
        {
            State = CommandExecutionState.Failed;
            throw;
        }
        finally
        {
            if (linked is not null && ReferenceEquals(_execution, linked))
            {
                _execution = null;
            }

            linked?.Dispose();
            IsRunning = false;
            if (!_pipeline.AllowsOverlap)
            {
                Interlocked.Exchange(ref _runLock, 0);
            }

            NotifyCanExecuteChanged();
        }
    }

    /// <summary>Requests cancellation of the in-flight execution.</summary>
    public void Cancel()
    {
        try
        {
            _execution?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // Execution already finished.
        }
    }

    /// <summary>Raises <see cref="CanExecuteChanged"/>.</summary>
    public void NotifyCanExecuteChanged()
        => NotificationMarshaller.Raise(
            () => _canExecuteChanged.Raise(this, EventArgs.Empty),
            _options.MainThread,
            _options.MarshalToMainThread);
}
