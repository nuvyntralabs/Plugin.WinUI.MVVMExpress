using System.Windows.Input;
using Plugin.WinUI.MVVMExpress.Input;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Input;

public sealed class WeakCanExecuteChangedTests
{
    [Fact]
    public void Raise_InvokesLiveHandler()
    {
        var raised = 0;
        var command = new ModelCommand(() => { });
        command.CanExecuteChanged += (_, _) => raised++;
        command.NotifyCanExecuteChanged();
        Assert.Equal(1, raised);
    }

    [Fact]
    public void Remove_StopsDelivery()
    {
        var raised = 0;
        var command = new ModelCommand(() => { });
        void Handler(object? sender, EventArgs e) => raised++;
        command.CanExecuteChanged += Handler;
        command.CanExecuteChanged -= Handler;
        command.NotifyCanExecuteChanged();
        Assert.Equal(0, raised);
    }

    [Fact]
    public void TypedAndAsyncCommands_UseTheSameWeakEvent()
    {
        var raised = 0;
        ICommand[] commands =
        [
            new ModelCommand<int>(_ => { }),
            new AsyncModelCommand(_ => Task.CompletedTask),
            new AsyncModelCommand<int>((_, _) => Task.CompletedTask)
        ];

        foreach (var command in commands)
        {
            command.CanExecuteChanged += (_, _) => raised++;
        }

        ((ModelCommand<int>)commands[0]).NotifyCanExecuteChanged();
        ((AsyncModelCommand)commands[1]).NotifyCanExecuteChanged();
        ((AsyncModelCommand<int>)commands[2]).NotifyCanExecuteChanged();
        Assert.Equal(3, raised);
    }

    [Fact]
    public void StaticHandler_RemainsUntilRemoved()
    {
        StaticSink.Count = 0;
        var command = new ModelCommand(() => { });
        command.CanExecuteChanged += StaticSink.OnChanged;
        command.NotifyCanExecuteChanged();
        command.CanExecuteChanged -= StaticSink.OnChanged;
        command.NotifyCanExecuteChanged();
        Assert.Equal(1, StaticSink.Count);
    }

    private static class StaticSink
    {
        public static int Count;

        public static void OnChanged(object? sender, EventArgs e) => Count++;
    }
}
