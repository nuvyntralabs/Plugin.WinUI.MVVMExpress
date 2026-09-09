using Plugin.WinUI.MVVMExpress.Input;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Input;

public sealed class CommandTests
{
    [Fact]
    public void ModelCommand_DoesNotRun_WhenCanExecuteFalse()
    {
        var ran = false;
        var command = new ModelCommand(() => ran = true, () => false);
        Assert.False(command.CanExecute(null));
        command.Execute(null);
        Assert.False(ran);
    }

    [Fact]
    public void ModelCommand_Runs_AndRaisesCanExecuteChanged()
    {
        var ran = false;
        var raised = 0;
        var command = new ModelCommand(() => ran = true);
        command.CanExecuteChanged += (_, _) => raised++;
        command.Execute(null);
        command.NotifyCanExecuteChanged();
        Assert.True(ran);
        Assert.Equal(1, raised);
    }

    [Fact]
    public void ModelCommandOfT_RejectsWrongType()
    {
        var seen = 0;
        var command = new ModelCommand<int>(value => seen = value);
        Assert.False(command.CanExecute("x"));
        command.Execute("x");
        Assert.Equal(0, seen);
        command.Execute(7);
        Assert.Equal(7, seen);
    }

    [Fact]
    public async Task AsyncCommand_Completes()
    {
        var command = new AsyncModelCommand(_ => Task.CompletedTask);
        await command.ExecuteAsync();
        Assert.Equal(CommandExecutionState.Completed, command.State);
        Assert.False(command.IsRunning);
    }

    [Fact]
    public async Task AsyncCommand_Failure_SetsFailed_AndRethrows()
    {
        var command = new AsyncModelCommand(_ => throw new InvalidOperationException("boom"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => command.ExecuteAsync());
        Assert.Equal(CommandExecutionState.Failed, command.State);
        Assert.False(command.IsRunning);
    }

    [Fact]
    public async Task AsyncCommand_PreventsConcurrentExecution()
    {
        var started = new TaskCompletionSource();
        var release = new TaskCompletionSource();
        var command = new AsyncModelCommand(async ct =>
        {
            started.TrySetResult();
            await release.Task.WaitAsync(ct);
        });

        var first = command.ExecuteAsync();
        await started.Task;
        var second = command.ExecuteAsync();
        await second;
        Assert.True(command.IsRunning);
        release.TrySetResult();
        await first;
        Assert.False(command.IsRunning);
        Assert.Equal(CommandExecutionState.Completed, command.State);
    }

    [Fact]
    public async Task AsyncCommandOfT_PassesParameter()
    {
        string? seen = null;
        var command = new AsyncModelCommand<string>((value, _) =>
        {
            seen = value;
            return Task.CompletedTask;
        });

        await command.ExecuteAsync("sku-1");
        Assert.Equal("sku-1", seen);
        Assert.Equal(CommandExecutionState.Completed, command.State);
    }

    [Fact]
    public void ModelCommand_NullExecute_Throws()
        => Assert.Throws<ArgumentNullException>(() => new ModelCommand(null!));

    [Fact]
    public void AsyncCommand_NullExecute_Throws()
        => Assert.Throws<ArgumentNullException>(() => new AsyncModelCommand(null!));

    [Fact]
    public async Task AsyncCommand_ExternalToken_Cancels()
    {
        var started = new TaskCompletionSource();
        var command = new AsyncModelCommand(async ct =>
        {
            started.TrySetResult();
            await Task.Delay(50_000, ct);
        });

        using var cts = new CancellationTokenSource();
        var run = command.ExecuteAsync(cts.Token);
        await started.Task;
        await cts.CancelAsync();
        await run;
        Assert.Equal(CommandExecutionState.Cancelled, command.State);
        Assert.False(command.IsRunning);
    }

    [Fact]
    public async Task AsyncCommand_CanExecute_FalseWhileRunning()
    {
        var started = new TaskCompletionSource();
        var release = new TaskCompletionSource();
        var command = new AsyncModelCommand(async ct =>
        {
            started.TrySetResult();
            await release.Task.WaitAsync(ct);
        });

        var run = command.ExecuteAsync();
        await started.Task;
        Assert.False(command.CanExecute(null));
        release.TrySetResult();
        await run;
        Assert.True(command.CanExecute(null));
    }

    [Fact]
    public async Task AsyncCommandOfT_RejectsWrongType()
    {
        var seen = 0;
        var command = new AsyncModelCommand<int>((value, _) =>
        {
            seen = value;
            return Task.CompletedTask;
        });

        Assert.False(command.CanExecute("x"));
        command.Execute("x");
        await Task.Yield();
        Assert.Equal(0, seen);
        await command.ExecuteAsync(4);
        Assert.Equal(4, seen);
    }
}
