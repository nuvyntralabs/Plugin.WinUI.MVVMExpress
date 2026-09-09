using Plugin.WinUI.MVVMExpress.ComponentModel;

namespace Plugin.WinUI.MVVMExpress.Busy;

/// <summary>Nested busy counter that restores correctly when disposed, including after exceptions.</summary>
public interface IBusyGate
{
    /// <summary>Gets a value indicating whether at least one <see cref="Enter"/> is outstanding.</summary>
    bool IsBusy { get; }

    /// <summary>Current nest depth.</summary>
    int Depth { get; }

    /// <summary>Increments depth. Dispose the return value to decrement.</summary>
    IDisposable Enter();
}

/// <summary>Thread-safe nested <see cref="IBusyGate"/>.</summary>
public sealed class BusyGate : ObservableModel, IBusyGate
{
    private int _depth;

    /// <inheritdoc />
    public bool IsBusy => Volatile.Read(ref _depth) > 0;

    /// <inheritdoc />
    public int Depth => Volatile.Read(ref _depth);

    /// <inheritdoc />
    public IDisposable Enter()
    {
        Interlocked.Increment(ref _depth);
        Notify(nameof(IsBusy));
        Notify(nameof(Depth));
        return new Scope(this);
    }

    private void Exit()
    {
        Interlocked.Decrement(ref _depth);
        Notify(nameof(IsBusy));
        Notify(nameof(Depth));
    }

    private sealed class Scope : IDisposable
    {
        private BusyGate? _owner;

        internal Scope(BusyGate owner) => _owner = owner;

        public void Dispose()
        {
            var owner = Interlocked.Exchange(ref _owner, null);
            owner?.Exit();
        }
    }
}
