using Plugin.WinUI.MVVMExpress.Diagnostics;
using Plugin.WinUI.MVVMExpress.Errors;
using Plugin.WinUI.MVVMExpress.Input;
using Plugin.WinUI.MVVMExpress.Outcome;
using Plugin.WinUI.MVVMExpress.Testing;
using Plugin.WinUI.MVVMExpress.Threading;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Input;

public sealed class CommandThreadingTests
{
    [Fact]
    public async Task AsyncCommand_AfterThreadPoolHop_MarshalsCanExecuteChanged()
    {
        var main = new RecordingMainThread { IsMainThread = false };
        using var scope = NotificationMarshaller.UseScope(main);
        var inline = 0;
        var hopped = 0;
        var command = new AsyncModelCommand(async ct => await Task.Yield());
        command.CanExecuteChanged += (_, _) =>
        {
            if (main.IsInvoking)
            {
                hopped++;
            }
            else
            {
                inline++;
            }
        };

        await command.ExecuteAsync();
        Assert.Equal(0, inline);
        Assert.True(hopped >= 1);
        Assert.True(main.InvokeCount >= 1);
    }

    [Fact]
    public async Task AsyncCommand_IsRunning_PropertyChanged_Hops()
    {
        var main = new RecordingMainThread { IsMainThread = false };
        using var scope = NotificationMarshaller.UseScope(main);
        var offThread = 0;
        var command = new AsyncModelCommand(async ct => await Task.Yield());
        command.PropertyChanged += (_, _) =>
        {
            if (!main.IsInvoking)
            {
                offThread++;
            }
        };

        await command.ExecuteAsync();
        Assert.Equal(0, offThread);
        Assert.True(main.InvokeCount >= 1);
    }

    [Fact]
    public async Task Execute_DoesNotThrow_AndPushesErrorSink()
    {
        var tcs = new TaskCompletionSource<ErrorInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = new AsyncModelCommand(
            _ => throw new InvalidOperationException("boom"),
            options: new AsyncCommandOptions { ErrorSink = new CallbackErrorSink(tcs) });

        var thrown = Record.Exception(() => command.Execute(null));
        Assert.Null(thrown);
        var error = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("E_CMD", error.Code);
        Assert.Equal(CommandExecutionState.Failed, command.State);
    }

    [Fact]
    public async Task ExecuteAsync_StillRethrows()
    {
        var command = new AsyncModelCommand(_ => throw new InvalidOperationException("boom"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => command.ExecuteAsync());
        Assert.Equal(CommandExecutionState.Failed, command.State);
    }

    [Fact]
    public void StrictMainThread_Throws_WhenCanExecuteChangedRaisedInline()
    {
        var main = new StrictMainThread();
        using var scope = NotificationMarshaller.UseScope(main, marshal: false);
        NotificationMarshaller.ThrowOnOffThreadRaise = true;
        try
        {
            var command = new ModelCommand(() => { });
            Assert.Throws<InvalidOperationException>(() => command.NotifyCanExecuteChanged());
        }
        finally
        {
            NotificationMarshaller.ThrowOnOffThreadRaise = false;
        }
    }

    [Fact]
    public void EnableDiagnostics_LogsThreadHop()
    {
        var traces = new List<string>();
        var main = new RecordingMainThread { IsMainThread = false };
        using var scope = NotificationMarshaller.UseScope(
            main,
            diagnostics: new CallbackDiagnostics((_, message) => traces.Add(message)));
        var command = new ModelCommand(() => { });
        command.NotifyCanExecuteChanged();
        Assert.Contains(traces, item => item.Contains("Hopping", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AsyncCommandOfT_Execute_DoesNotThrow()
    {
        var tcs = new TaskCompletionSource<ErrorInfo>(TaskCreationOptions.RunContinuationsAsynchronously);
        var command = new AsyncModelCommand<int>(
            (_, _) => throw new InvalidOperationException("typed"),
            options: new AsyncCommandOptions { ErrorSink = new CallbackErrorSink(tcs) });
        var thrown = Record.Exception(() => command.Execute(1));
        Assert.Null(thrown);
        var error = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("E_CMD", error.Code);
    }

    private sealed class CallbackErrorSink(TaskCompletionSource<ErrorInfo> tcs) : IErrorSink
    {
        public Task HandleAsync(ErrorInfo error, CancellationToken cancellationToken = default)
        {
            tcs.TrySetResult(error);
            return Task.CompletedTask;
        }
    }
}
