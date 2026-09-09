using Plugin.WinUI.MVVMExpress.Input;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Input;

public sealed class AsyncCommandOptionsTests
{
    [Fact]
    public async Task Timeout_Cancels()
    {
        var command = new AsyncModelCommand(
            ct => Task.Delay(TimeSpan.FromSeconds(30), ct),
            options: new AsyncCommandOptions { Timeout = TimeSpan.FromMilliseconds(20) });
        await command.ExecuteAsync();
        Assert.Equal(CommandExecutionState.Cancelled, command.State);
    }

    [Fact]
    public async Task Retry_RunsUntilSuccess()
    {
        var attempts = 0;
        var command = new AsyncModelCommand(
            _ =>
            {
                attempts++;
                if (attempts < 3)
                {
                    throw new InvalidOperationException("retry");
                }

                return Task.CompletedTask;
            },
            options: new AsyncCommandOptions { RetryCount = 2, RetryDelay = TimeSpan.Zero });
        await command.ExecuteAsync();
        Assert.Equal(3, attempts);
        Assert.Equal(CommandExecutionState.Completed, command.State);
    }

    [Fact]
    public async Task CancelPrevious_StartsSecondRun()
    {
        var started = 0;
        var releaseFirst = new TaskCompletionSource();
        var command = new AsyncModelCommand(
            async ct =>
            {
                var n = Interlocked.Increment(ref started);
                if (n == 1)
                {
                    await releaseFirst.Task.WaitAsync(ct);
                }
            },
            options: new AsyncCommandOptions { Concurrency = ConcurrencyMode.CancelPrevious });

        var first = command.ExecuteAsync();
        await Task.Delay(10);
        Assert.True(command.CanExecute(null));
        var second = command.ExecuteAsync();
        await first;
        await second;
        Assert.Equal(2, started);
        Assert.Equal(CommandExecutionState.Completed, command.State);
    }

    [Fact]
    public async Task Generic_Retry_AppliesOptions()
    {
        var attempts = 0;
        var command = new AsyncModelCommand<int>(
            (_, _) =>
            {
                attempts++;
                if (attempts < 2)
                {
                    throw new InvalidOperationException("retry");
                }

                return Task.CompletedTask;
            },
            options: new AsyncCommandOptions { RetryCount = 1, RetryDelay = TimeSpan.Zero });
        await command.ExecuteAsync(1);
        Assert.Equal(2, attempts);
        Assert.Equal(CommandExecutionState.Completed, command.State);
    }
}
