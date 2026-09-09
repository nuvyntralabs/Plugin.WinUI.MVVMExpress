using Plugin.WinUI.MVVMExpress.Busy;

namespace Plugin.WinUI.MVVMExpress.Core.Tests.Busy;

public sealed class BusyGateTests
{
    [Fact]
    public void NestedEnter_StaysBusy_UntilAllDisposed()
    {
        var gate = new BusyGate();
        Assert.False(gate.IsBusy);
        var first = gate.Enter();
        var second = gate.Enter();
        Assert.True(gate.IsBusy);
        Assert.Equal(2, gate.Depth);
        first.Dispose();
        Assert.True(gate.IsBusy);
        second.Dispose();
        Assert.False(gate.IsBusy);
        Assert.Equal(0, gate.Depth);
    }

    [Fact]
    public void Enter_Restores_WhenExceptionThrown()
    {
        var gate = new BusyGate();
        try
        {
            using (gate.Enter())
            {
                throw new InvalidOperationException();
            }
        }
        catch (InvalidOperationException)
        {
            // expected
        }

        Assert.False(gate.IsBusy);
    }

    [Fact]
    public void DisposeTwice_DoesNotGoNegative()
    {
        var gate = new BusyGate();
        var scope = gate.Enter();
        scope.Dispose();
        scope.Dispose();
        Assert.Equal(0, gate.Depth);
        Assert.False(gate.IsBusy);
    }
}
