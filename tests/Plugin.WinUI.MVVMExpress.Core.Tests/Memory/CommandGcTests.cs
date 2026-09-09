using System.Windows.Input;
using Plugin.WinUI.MVVMExpress.Core.Tests.Support;
using Plugin.WinUI.MVVMExpress.Input;
using Plugin.WinUI.MVVMExpress.Testing;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Memory;

public sealed class CommandGcTests
{
    [Fact]
    public void Command_IsCollectable_WithOwningViewModel()
    {
        var (vm, command) = Create();
        Assert.True(LeakProbe.IsCollected(vm), "ViewModel owning a command was not collected.");
        Assert.True(LeakProbe.IsCollected(command), "ModelCommand was not collected with its ViewModel.");
    }

    [Fact]
    public async Task AsyncCommand_Cancel_StopsWork()
    {
        var vm = new ProbeViewModel();
        var run = vm.WaitCommand.ExecuteAsync();
        Assert.True(vm.WaitCommand.IsRunning);
        vm.WaitCommand.Cancel();
        await run;
        Assert.False(vm.WaitCommand.IsRunning);
        Assert.Equal(CommandExecutionState.Cancelled, vm.WaitCommand.State);
        vm.Dispose();
    }

    [Fact]
    public void CommandBoundToHandler_DoesNotPinViewModel()
    {
        var (vm, _) = BindAndRelease();
        Assert.True(LeakProbe.IsCollected(vm), "ViewModel bound to CanExecuteChanged was not collected.");
    }

    [Fact]
    public void CommandBoundToButtonThenPopPage_DoesNotPinPage()
    {
        var (page, button, command) = BindButtonThenPop();
        Assert.True(LeakProbe.IsCollected(page), "Popped page stayed alive through CanExecuteChanged.");
        Assert.True(LeakProbe.IsCollected(button), "Button-shaped subscriber stayed alive through CanExecuteChanged.");
        GC.KeepAlive(command);
    }

    private static (WeakReference Vm, WeakReference Command) BindAndRelease()
    {
        var vm = new ProbeViewModel();
        void Handler(object? sender, EventArgs e)
        {
            _ = vm.Name;
        }

        vm.PingCommand.CanExecuteChanged += Handler;
        vm.PingCommand.NotifyCanExecuteChanged();
        vm.PingCommand.CanExecuteChanged -= Handler;
        var vmRef = LeakProbe.Track(vm);
        var cmdRef = LeakProbe.Track(vm.PingCommand);
        vm.Dispose();
        return (vmRef, cmdRef);
    }

    private static (WeakReference Page, WeakReference Button, ICommand Command) BindButtonThenPop()
    {
        var vm = new ProbeViewModel();
        var page = new FakePage { BindingContext = vm };
        var button = new ButtonShapedBinder(vm.PingCommand, page);
        page.Child = button;
        var pageRef = LeakProbe.Track(page);
        var buttonRef = LeakProbe.Track(button);
        return (pageRef, buttonRef, vm.PingCommand);
    }

    private static (WeakReference Vm, WeakReference Command) Create()
    {
        var vm = new ProbeViewModel();
        vm.PingCommand.Execute(null);
        var vmRef = LeakProbe.Track(vm);
        var cmdRef = LeakProbe.Track(vm.PingCommand);
        vm.Dispose();
        return (vmRef, cmdRef);
    }

    private sealed class FakePage
    {
        public object? BindingContext { get; set; }

        public object? Child { get; set; }
    }

    /// <summary>Mirrors a MAUI Button: instance CanExecuteChanged handler, no unsubscribe on pop.</summary>
    private sealed class ButtonShapedBinder
    {
        private readonly object _page;

        public ButtonShapedBinder(ICommand command, object page)
        {
            _page = page;
            command.CanExecuteChanged += OnCommandCanExecuteChanged;
            _ = command.CanExecute(null);
        }

        private void OnCommandCanExecuteChanged(object? sender, EventArgs e)
        {
            if (sender is ICommand command)
            {
                _ = command.CanExecute(null);
            }

            _ = _page;
        }
    }
}
