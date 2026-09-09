using Plugin.WinUI.MVVMExpress.Input;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Input;

public sealed class CommandPipelineTests
{
    [Fact]
    public async Task Queue_RunsBoth()
    {
        var started = new TaskCompletionSource();
        var release = new TaskCompletionSource();
        var runs = 0;
        var command = new AsyncModelCommand(
            async ct =>
            {
                var n = Interlocked.Increment(ref runs);
                if (n == 1)
                {
                    started.TrySetResult();
                    await release.Task.WaitAsync(ct);
                }
            },
            options: new AsyncCommandOptions { Concurrency = ConcurrencyMode.Queue });

        var first = command.ExecuteAsync();
        await started.Task;
        var second = command.ExecuteAsync();
        release.TrySetResult();
        await Task.WhenAll(first, second);
        Assert.Equal(2, runs);
        Assert.Equal(CommandExecutionState.Completed, command.State);
    }

    [Fact]
    public async Task Debounce_RunsOnce()
    {
        var runs = 0;
        var command = new AsyncModelCommand(
            _ =>
            {
                Interlocked.Increment(ref runs);
                return Task.CompletedTask;
            },
            options: new AsyncCommandOptions { Debounce = TimeSpan.FromMilliseconds(200) });

        var first = command.ExecuteAsync();
        var second = command.ExecuteAsync();
        var third = command.ExecuteAsync();
        await Task.WhenAll(first, second, third);
        Assert.Equal(1, runs);
    }

    [Fact]
    public async Task Throttle_SkipsSecond()
    {
        var runs = 0;
        var command = new AsyncModelCommand(
            _ =>
            {
                Interlocked.Increment(ref runs);
                return Task.CompletedTask;
            },
            options: new AsyncCommandOptions { Throttle = TimeSpan.FromSeconds(5) });

        await command.ExecuteAsync();
        await command.ExecuteAsync();
        Assert.Equal(1, runs);
    }

    [Fact]
    public async Task Allow_RunsConcurrently()
    {
        var started = 0;
        var release = new TaskCompletionSource();
        var command = new AsyncModelCommand(
            async ct =>
            {
                Interlocked.Increment(ref started);
                await release.Task.WaitAsync(ct);
            },
            options: new AsyncCommandOptions { Concurrency = ConcurrencyMode.Allow });

        var first = command.ExecuteAsync();
        var second = command.ExecuteAsync();
        await WaitUntil(() => Volatile.Read(ref started) == 2);
        release.TrySetResult();
        await Task.WhenAll(first, second);
        Assert.Equal(2, started);
    }

    private static async Task WaitUntil(Func<bool> condition)
    {
        for (var i = 0; i < 50 && !condition(); i++)
        {
            await Task.Delay(10);
        }

        Assert.True(condition());
    }
}
