using Plugin.WinUI.MVVMExpress.Busy;
using Plugin.WinUI.MVVMExpress.ComponentModel;
using Plugin.WinUI.MVVMExpress.Errors;
using Plugin.WinUI.MVVMExpress.Outcome;
using Plugin.WinUI.MVVMExpress.State;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.ComponentModel;

public sealed class ExecuteAsyncTests
{
    [Fact]
    public async Task ExecuteAsync_Success_ReturnsOutcome()
    {
        var vm = new Probe();
        var result = await vm.RunAsync(_ => Task.CompletedTask);
        Assert.True(result.IsSuccess);
        Assert.False(vm.Busy.IsBusy);
    }

    [Fact]
    public async Task ExecuteAsync_Exception_GoesToSink()
    {
        var sink = new Recording();
        var vm = new Probe(sink);
        var result = await vm.RunAsync(_ => throw new InvalidOperationException("boom"));
        Assert.False(result.IsSuccess);
        Assert.Equal("E_OP", result.Error?.Code);
        Assert.Equal(ViewModelStatus.Error, vm.Status);
        Assert.Single(sink.Errors);
    }

    private sealed class Recording : IErrorSink
    {
        public List<ErrorInfo> Errors { get; } = [];

        public Task HandleAsync(ErrorInfo error, CancellationToken cancellationToken = default)
        {
            Errors.Add(error);
            return Task.CompletedTask;
        }
    }

    private sealed class Probe : ViewModel
    {
        public Probe(IErrorSink? errors = null)
            : base(errors, new BusyGate())
        {
            Busy = (BusyGate)base.Busy!;
        }

        public new BusyGate Busy { get; }

        public Task<Plugin.WinUI.MVVMExpress.Outcome.Outcome> RunAsync(Func<CancellationToken, Task> work)
            => ExecuteAsync(work);
    }
}
